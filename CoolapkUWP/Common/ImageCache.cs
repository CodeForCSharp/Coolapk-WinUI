using CoolapkUWP.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Extensions.Logging;

namespace CoolapkUWP.Common
{
    public class ImageCache
    {
        private const long MemoryCacheMaxBytes = 96L * 1024 * 1024;
        private const long MaxCacheableImageBytes = 16 * 1024 * 1024;
        private const int MaxDecodePixelWidth = 1280;
        private const long MaxDecodePixels = 4L * 1024 * 1024;
        private const long DiskCacheMaxBytes = 512L * 1024 * 1024;
        private static readonly TimeSpan DiskCacheMaxAge = TimeSpan.FromDays(30);

        private static readonly Lazy<ImageCache> _instance = new Lazy<ImageCache>(() => new ImageCache());

        public static ImageCache Instance => _instance.Value;

        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromDays(7);

        private Task<StorageFolder> _cacheFolderTask;
        private Task _maintainTask;
        private readonly HttpClient _httpClient;

        // 强缓存会被 UI 线程（读）与后台解码线程（写）同时访问，所有访问均需持有 _cacheLock。
        private readonly object _cacheLock = new object();
        private readonly Dictionary<string, BitmapImage> _strongCache = new Dictionary<string, BitmapImage>();
        private readonly LinkedList<string> _lru = new LinkedList<string>();
        private readonly Dictionary<string, LinkedListNode<string>> _lruNodes = new Dictionary<string, LinkedListNode<string>>();
        // 按文件名聚合的宽度索引：fileName -> (decodePixelWidth -> cacheKey)，用于 O(宽度数) 查找最佳已缓存尺寸。
        private readonly Dictionary<string, SortedDictionary<int, string>> _cacheKeysByFile = new Dictionary<string, SortedDictionary<int, string>>();
        private long _strongCacheBytes;

        private readonly ConcurrentDictionary<string, Task<BitmapImage>> _inflightDecodes = new ConcurrentDictionary<string, Task<BitmapImage>>();
        private readonly ConcurrentDictionary<string, Task<StorageFile>> _inflightDownloads = new ConcurrentDictionary<string, Task<StorageFile>>();

        private static SemaphoreSlim _decodeSemaphore = new SemaphoreSlim(Math.Max(1, SettingsHelper.Get<int>(SettingsHelper.SemaphoreSlimCount)));

        public static void SetDecodeSemaphore(int initialCount)
        {
            Interlocked.Exchange(ref _decodeSemaphore, new SemaphoreSlim(Math.Max(1, initialCount)));
        }

        public ImageCache()
        {
            _httpClient = new HttpClient();
            CopyHeadersFromNetworkHelper();
        }

        private void CopyHeadersFromNetworkHelper()
        {
            var headers = _httpClient.DefaultRequestHeaders;
            headers.Clear();
            foreach (var header in NetworkHelper.Client.DefaultRequestHeaders)
            {
                headers.Add(header.Key, header.Value);
            }
            headers.UserAgent.Clear();
            foreach (var ua in NetworkHelper.Client.DefaultRequestHeaders.UserAgent)
            {
                headers.UserAgent.Add(ua);
            }
        }

        private Task<StorageFolder> GetCacheFolderAsync()
        {
            return _cacheFolderTask ??= GetCacheFolderCoreAsync();
        }

        private static async Task<StorageFolder> GetCacheFolderCoreAsync()
        {
            StorageFolder root = ApplicationData.Current.LocalCacheFolder;
            string path = Path.Combine(root.Path, "ImageCache");
            await root.CreateFolderAsync("ImageCache", CreationCollisionOption.OpenIfExists);
            return await StorageFolder.GetFolderFromPathAsync(path);
        }

        private static string GetCacheFileName(Uri uri)
        {
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(uri.ToString()));
            return Convert.ToHexString(hash);
        }

        private static string GetCacheKey(string fileName, int decodePixelWidth) => $"{fileName}|{decodePixelWidth}";

        public async Task<BitmapImage> GetBitmapAsync(Uri uri, int decodePixelWidth)
        {
            if (decodePixelWidth > MaxDecodePixelWidth) { decodePixelWidth = MaxDecodePixelWidth; }

            string fileName = GetCacheFileName(uri);
            string key = GetCacheKey(fileName, decodePixelWidth);

            BitmapImage cached = GetFromStrongCache(key);
            if (cached == null && decodePixelWidth > 0) { cached = GetBestFromStrongCache(fileName, decodePixelWidth); }
            if (cached != null) { return cached; }

            if (_inflightDecodes.TryGetValue(key, out Task<BitmapImage> pending)) { return await pending; }

            Task<BitmapImage> task = LoadAndCacheAsync(uri, fileName, decodePixelWidth, key);
            _inflightDecodes[key] = task;
            try
            {
                return await task;
            }
            finally
            {
                _inflightDecodes.TryRemove(key, out _);
            }
        }

        private async Task<BitmapImage> LoadAndCacheAsync(Uri uri, string fileName, int decodePixelWidth, string key)
        {
            StorageFile file = await GetFileFromCacheAsync(uri);
            BitmapImage bitmap = await DecodeAndCacheAsync(file, fileName, decodePixelWidth, key);
            if (bitmap != null) { return bitmap; }

            await file.DeleteAsync();
            StorageFile fresh = await GetFileFromCacheAsync(uri);
            bitmap = await DecodeAndCacheAsync(fresh, fileName, decodePixelWidth, key);
            if (bitmap != null) { return bitmap; }

            await fresh.DeleteAsync();
            return null;
        }

        private async Task<BitmapImage> DecodeAndCacheAsync(StorageFile file, string fileName, int decodePixelWidth, string key)
        {
            BitmapImage bitmap = await DecodeImageAsync(file, decodePixelWidth);
            if (bitmap != null)
            {
                AddToStrongCache(fileName, decodePixelWidth, key, bitmap);
            }
            return bitmap;
        }

        private static async Task<BitmapImage> DecodeImageAsync(StorageFile file, int decodePixelWidth)
        {
            await _decodeSemaphore.WaitAsync();
            try
            {
                try
                {
                    var bitmap = new BitmapImage();
                    using (IRandomAccessStream stream = await file.OpenReadAsync())
                    {
                        (int width, int height) = await GetDecodeDimensionsAsync(stream, decodePixelWidth);
                        if (width > 0 && height > 0)
                        {
                            bitmap.DecodePixelWidth = width;
                            bitmap.DecodePixelHeight = height;
                        }
                        stream.Seek(0);
                        await bitmap.SetSourceAsync(stream);
                    }
                    return bitmap;
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.CreateLogger(nameof(ImageCache)).LogWarning(ex, ex.ExceptionToMessage());
                    return null;
                }
            }
            finally
            {
                _decodeSemaphore.Release();
            }
        }

        private static async Task<(int Width, int Height)> GetDecodeDimensionsAsync(IRandomAccessStream stream, int decodePixelWidth)
        {
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            int sourceWidth = (int)decoder.PixelWidth;
            int sourceHeight = (int)decoder.PixelHeight;
            if (sourceWidth <= 0 || sourceHeight <= 0) { return (0, 0); }

            double scale = 1.0;
            if (decodePixelWidth > 0)
            {
                if (sourceHeight > sourceWidth * 1.5)
                {
                    decodePixelWidth = Math.Max(decodePixelWidth / 2, 256);
                }
                if (sourceWidth > decodePixelWidth)
                {
                    scale = (double)decodePixelWidth / sourceWidth;
                }
            }

            int frameCount = Math.Max(1, (int)decoder.FrameCount);
            double sourceArea = (double)sourceWidth * sourceHeight * frameCount;
            if (sourceArea > MaxDecodePixels)
            {
                double areaScale = Math.Sqrt((double)MaxDecodePixels / sourceArea);
                if (areaScale < scale) { scale = areaScale; }
            }

            int width = Math.Max(1, (int)Math.Round(sourceWidth * scale));
            int height = Math.Max(1, (int)Math.Round(sourceHeight * scale));
            if (width == sourceWidth && height == sourceHeight) { return (0, 0); }
            return (width, height);
        }

        private BitmapImage GetFromStrongCache(string key)
        {
            lock (_cacheLock)
            {
                if (_strongCache.TryGetValue(key, out BitmapImage bitmap))
                {
                    TouchLru(key);
                    return bitmap;
                }
                return null;
            }
        }

        private BitmapImage GetBestFromStrongCache(string fileName, int decodePixelWidth)
        {
            lock (_cacheLock)
            {
                if (!_cacheKeysByFile.TryGetValue(fileName, out SortedDictionary<int, string> byWidth))
                {
                    return null;
                }

                // 升序遍历，取第一个宽度 >= 目标宽度的缓存（即最小可用尺寸）。
                foreach (KeyValuePair<int, string> entry in byWidth)
                {
                    if (entry.Key < decodePixelWidth) { continue; }
                    if (_strongCache.TryGetValue(entry.Value, out BitmapImage bitmap))
                    {
                        TouchLru(entry.Value);
                        return bitmap;
                    }
                }
                return null;
            }
        }

        private void AddToStrongCache(string fileName, int decodePixelWidth, string key, BitmapImage bitmap)
        {
            lock (_cacheLock)
            {
                if (_strongCache.ContainsKey(key))
                {
                    TouchLru(key);
                    return;
                }

                long size = EstimateBytes(bitmap);
                if (size <= 0 || size > MaxCacheableImageBytes) { return; }

                _strongCache[key] = bitmap;
                _lruNodes[key] = _lru.AddFirst(key);
                _strongCacheBytes += size;

                if (!_cacheKeysByFile.TryGetValue(fileName, out SortedDictionary<int, string> byWidth))
                {
                    byWidth = new SortedDictionary<int, string>();
                    _cacheKeysByFile[fileName] = byWidth;
                }
                byWidth[decodePixelWidth] = key;

                EvictStrongCacheIfNeeded();
            }
        }

        private void TouchLru(string key)
        {
            if (_lruNodes.TryGetValue(key, out LinkedListNode<string> node))
            {
                _lru.Remove(node);
                _lru.AddFirst(node);
            }
        }

        private void EvictStrongCacheIfNeeded()
        {
            while (_strongCacheBytes > MemoryCacheMaxBytes && _lru.Count > 0)
            {
                LinkedListNode<string> node = _lru.Last;
                string key = node.Value;
                BitmapImage victim = _strongCache[key];
                _strongCache.Remove(key);
                _lruNodes.Remove(key);
                _lru.RemoveLast();
                _strongCacheBytes -= EstimateBytes(victim);
                RemoveFromFileIndex(key);
            }
        }

        private void RemoveFromFileIndex(string key)
        {
            int separator = key.LastIndexOf('|');
            if (separator <= 0) { return; }
            string fileName = key.Substring(0, separator);
            if (!int.TryParse(key.Substring(separator + 1), out int width)) { return; }

            if (_cacheKeysByFile.TryGetValue(fileName, out SortedDictionary<int, string> byWidth))
            {
                byWidth.Remove(width);
                if (byWidth.Count == 0) { _cacheKeysByFile.Remove(fileName); }
            }
        }

        private static long EstimateBytes(BitmapImage bitmap)
        {
            if (bitmap == null || bitmap.PixelWidth <= 0 || bitmap.PixelHeight <= 0) { return 0; }
            return (long)bitmap.PixelWidth * bitmap.PixelHeight * 4;
        }

        public async Task<StorageFile> GetFileFromCacheAsync(Uri uri)
        {
            _ = EnsureMaintainCacheAsync();

            var folder = await GetCacheFolderAsync();
            string fileName = GetCacheFileName(uri);
            var file = await folder.TryGetItemAsync(fileName) as StorageFile;

            if (file != null)
            {
                var meta = await folder.TryGetItemAsync(GetMetaFileName(fileName)) as StorageFile;
                var freshItem = meta ?? file;
                var props = await freshItem.GetBasicPropertiesAsync();
                if (DateTimeOffset.Now - props.DateModified < CacheDuration)
                    return file;
            }

            return await DownloadToFileAsync(uri, folder, fileName);
        }

        private async Task<StorageFile> DownloadToFileAsync(Uri uri, StorageFolder folder, string fileName)
        {
            Task<StorageFile> task = _inflightDownloads.GetOrAdd(fileName, _ => DownloadToFileCoreAsync(uri, folder, fileName));
            try
            {
                return await task;
            }
            finally
            {
                _inflightDownloads.TryRemove(new KeyValuePair<string, Task<StorageFile>>(fileName, task));
            }
        }

        private async Task<StorageFile> DownloadToFileCoreAsync(Uri uri, StorageFolder folder, string fileName)
        {
            var existing = await folder.TryGetItemAsync(fileName) as StorageFile;

            using var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (existing != null)
            {
                (string etag, DateTimeOffset lastModified) = await ReadMetaAsync(folder, fileName);
                if (!string.IsNullOrEmpty(etag) && EntityTagHeaderValue.TryParse(etag, out EntityTagHeaderValue etagValue))
                {
                    request.Headers.IfNoneMatch.Add(etagValue);
                }
                if (lastModified != DateTimeOffset.MinValue)
                {
                    request.Headers.IfModifiedSince = lastModified;
                }
            }

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (response.StatusCode == HttpStatusCode.NotModified && existing != null)
            {
                await WriteMetaAsync(folder, fileName, response.Headers.ETag?.ToString(), response.Content.Headers.LastModified);
                return existing;
            }

            response.EnsureSuccessStatusCode();

            StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            using (var networkStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = await file.OpenStreamForWriteAsync())
            {
                await networkStream.CopyToAsync(fileStream);
            }

            await WriteMetaAsync(folder, fileName, response.Headers.ETag?.ToString(), response.Content.Headers.LastModified);
            return file;
        }

        private static string GetMetaFileName(string fileName) => fileName + ".meta";

        private static async Task<(string ETag, DateTimeOffset LastModified)> ReadMetaAsync(StorageFolder folder, string fileName)
        {
            try
            {
                var meta = await folder.TryGetItemAsync(GetMetaFileName(fileName)) as StorageFile;
                if (meta == null) { return (null, DateTimeOffset.MinValue); }

                string json = await FileIO.ReadTextAsync(meta);
                JsonNode node = JsonNode.Parse(json);
                string etag = node?["etag"]?.GetValue<string>();
                string lastModified = node?["lastModified"]?.GetValue<string>();
                return (etag, DateTimeOffset.TryParse(lastModified, out DateTimeOffset lm) ? lm : DateTimeOffset.MinValue);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(ImageCache)).LogWarning(ex, ex.ExceptionToMessage());
                return (null, DateTimeOffset.MinValue);
            }
        }

        private static async Task WriteMetaAsync(StorageFolder folder, string fileName, string etag, DateTimeOffset? lastModified)
        {
            try
            {
                JsonObject obj = new JsonObject();
                if (!string.IsNullOrEmpty(etag)) { obj["etag"] = etag; }
                if (lastModified.HasValue) { obj["lastModified"] = lastModified.Value.ToString("O"); }
                if (obj.Count == 0) { return; }

                StorageFile meta = await folder.CreateFileAsync(GetMetaFileName(fileName), CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(meta, obj.ToJsonString());
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(ImageCache)).LogDebug(ex, ex.ExceptionToMessage());
            }
        }

        private Task EnsureMaintainCacheAsync()
        {
            if (_maintainTask == null)
            {
                _maintainTask = Task.Run(MaintainCacheAsync);
            }
            return _maintainTask;
        }

        private async Task MaintainCacheAsync()
        {
            try
            {
                var folder = await GetCacheFolderAsync();
                var files = await folder.GetFilesAsync();

                List<(StorageFile File, DateTimeOffset Modified, long Size)> entries = new List<(StorageFile, DateTimeOffset, long)>();
                long totalBytes = 0;
                foreach (var file in files)
                {
                    try
                    {
                        var props = await file.GetBasicPropertiesAsync();
                        long size = (long)props.Size;
                        entries.Add((file, props.DateModified, size));
                        totalBytes += size;
                    }
                    catch (Exception ex)
                    {
                        SettingsHelper.LogManager.CreateLogger(nameof(ImageCache)).LogDebug(ex, ex.ExceptionToMessage());
                    }
                }

                DateTimeOffset now = DateTimeOffset.Now;

                foreach (var (file, _, size) in entries.Where(e => now - e.Modified > DiskCacheMaxAge).ToList())
                {
                    try
                    {
                        await file.DeleteAsync();
                        totalBytes -= size;
                    }
                    catch (Exception ex)
                    {
                        SettingsHelper.LogManager.CreateLogger(nameof(ImageCache)).LogDebug(ex, ex.ExceptionToMessage());
                    }
                }

                if (totalBytes > DiskCacheMaxBytes)
                {
                    foreach (var (file, _, size) in entries.Where(e => now - e.Modified <= DiskCacheMaxAge).OrderBy(e => e.Modified))
                    {
                        if (totalBytes <= DiskCacheMaxBytes) { break; }
                        try
                        {
                            await file.DeleteAsync();
                            totalBytes -= size;
                        }
                        catch (Exception ex)
                        {
                            SettingsHelper.LogManager.CreateLogger(nameof(ImageCache)).LogDebug(ex, ex.ExceptionToMessage());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(ImageCache)).LogDebug(ex, ex.ExceptionToMessage());
            }
        }

        public async Task RemoveAsync(Uri[] uris)
        {
            var folder = await GetCacheFolderAsync();
            foreach (var uri in uris)
            {
                string fileName = GetCacheFileName(uri);
                foreach (string name in new[] { fileName, GetMetaFileName(fileName) })
                {
                    var file = await folder.TryGetItemAsync(name) as StorageFile;
                    if (file != null)
                        await file.DeleteAsync();
                }
            }
        }

        public async Task ClearAsync()
        {
            var folder = await GetCacheFolderAsync();
            var files = await folder.GetFilesAsync();
            foreach (var file in files)
            {
                await file.DeleteAsync();
            }

            lock (_cacheLock)
            {
                _strongCache.Clear();
                _lru.Clear();
                _lruNodes.Clear();
                _cacheKeysByFile.Clear();
                _strongCacheBytes = 0;
            }
        }
    }
}
