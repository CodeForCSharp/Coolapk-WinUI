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
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;

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

        private readonly Dictionary<string, BitmapImage> _strongCache = new Dictionary<string, BitmapImage>();
        private readonly LinkedList<string> _lru = new LinkedList<string>();
        private readonly Dictionary<string, LinkedListNode<string>> _lruNodes = new Dictionary<string, LinkedListNode<string>>();
        private long _strongCacheBytes;

        private readonly ConcurrentDictionary<string, Task<BitmapImage>> _inflightDecodes = new ConcurrentDictionary<string, Task<BitmapImage>>();
        private readonly ConcurrentDictionary<string, Task<StorageFile>> _inflightDownloads = new ConcurrentDictionary<string, Task<StorageFile>>();

        private static readonly SemaphoreSlim _decodeSemaphore = new SemaphoreSlim(4);

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

        public async Task<BitmapImage> GetBitmapAsync(Uri uri, int decodePixelWidth, DispatcherQueue dispatcher)
        {
            await dispatcher.ResumeForegroundAsync();

            if (decodePixelWidth > MaxDecodePixelWidth) { decodePixelWidth = MaxDecodePixelWidth; }

            string fileName = GetCacheFileName(uri);
            string key = GetCacheKey(fileName, decodePixelWidth);

            BitmapImage cached = GetFromStrongCache(key);
            if (cached == null && decodePixelWidth > 0) { cached = GetBestFromStrongCache(fileName, decodePixelWidth); }
            if (cached != null) { return cached; }

            if (_inflightDecodes.TryGetValue(key, out Task<BitmapImage> pending)) { return await pending; }

            Task<BitmapImage> task = LoadAndCacheAsync(uri, decodePixelWidth, key, dispatcher);
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

        private async Task<BitmapImage> LoadAndCacheAsync(Uri uri, int decodePixelWidth, string key, DispatcherQueue dispatcher)
        {
            await ThreadSwitcher.ResumeBackgroundAsync();

            StorageFile file = await GetFileFromCacheAsync(uri);
            BitmapImage bitmap = await DecodeAndCacheAsync(file, decodePixelWidth, key, dispatcher);
            if (bitmap != null) { return bitmap; }

            await ThreadSwitcher.ResumeBackgroundAsync();
            await file.DeleteAsync();
            StorageFile fresh = await GetFileFromCacheAsync(uri);
            bitmap = await DecodeAndCacheAsync(fresh, decodePixelWidth, key, dispatcher);
            if (bitmap != null) { return bitmap; }

            await fresh.DeleteAsync();
            return null;
        }

        private async Task<BitmapImage> DecodeAndCacheAsync(StorageFile file, int decodePixelWidth, string key, DispatcherQueue dispatcher)
        {
            await dispatcher.ResumeForegroundAsync();
            BitmapImage bitmap = await DecodeImageAsync(file, decodePixelWidth);
            if (bitmap != null)
            {
                AddToStrongCache(key, bitmap);
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
                    (int width, int height) = await GetDecodeDimensionsAsync(file, decodePixelWidth);
                    var bitmap = new BitmapImage();
                    if (width > 0 && height > 0)
                    {
                        bitmap.DecodePixelWidth = width;
                        bitmap.DecodePixelHeight = height;
                    }
                    using (var stream = await file.OpenReadAsync())
                    {
                        await bitmap.SetSourceAsync(stream);
                    }
                    return bitmap;
                }
                catch { return null; }
            }
            finally
            {
                _decodeSemaphore.Release();
            }
        }

        private static async Task<(int Width, int Height)> GetDecodeDimensionsAsync(StorageFile file, int decodePixelWidth)
        {
            using var stream = await file.OpenReadAsync();
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
            int sourceWidth = (int)decoder.PixelWidth;
            int sourceHeight = (int)decoder.PixelHeight;
            if (sourceWidth <= 0 || sourceHeight <= 0) { return (0, 0); }

            double scale = 1.0;
            if (decodePixelWidth > 0 && sourceWidth > decodePixelWidth)
            {
                scale = (double)decodePixelWidth / sourceWidth;
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
            if (_strongCache.TryGetValue(key, out BitmapImage bitmap))
            {
                TouchLru(key);
                return bitmap;
            }
            return null;
        }

        private BitmapImage GetBestFromStrongCache(string fileName, int decodePixelWidth)
        {
            string bestKey = null;
            int bestWidth = int.MaxValue;
            BitmapImage best = null;
            foreach (KeyValuePair<string, BitmapImage> entry in _strongCache)
            {
                if (!entry.Key.StartsWith(fileName + "|", StringComparison.Ordinal)) { continue; }
                if (!int.TryParse(entry.Key.Substring(fileName.Length + 1), out int width)) { continue; }
                if (width < decodePixelWidth || width >= bestWidth) { continue; }
                bestKey = entry.Key;
                bestWidth = width;
                best = entry.Value;
            }
            if (best != null && bestKey != null) { TouchLru(bestKey); }
            return best;
        }

        private void AddToStrongCache(string key, BitmapImage bitmap)
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
            EvictStrongCacheIfNeeded();
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
                BitmapImage victim = _strongCache[node.Value];
                _strongCache.Remove(node.Value);
                _lruNodes.Remove(node.Value);
                _lru.RemoveLast();
                _strongCacheBytes -= EstimateBytes(victim);
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
            catch { return (null, DateTimeOffset.MinValue); }
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
            catch { }
        }

        private Task EnsureMaintainCacheAsync()
        {
            if (_maintainTask == null)
            {
                _maintainTask = MaintainCacheAsync();
            }
            return _maintainTask;
        }

        private async Task MaintainCacheAsync()
        {
            try
            {
                await ThreadSwitcher.ResumeBackgroundAsync();

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
                    catch { }
                }

                DateTimeOffset now = DateTimeOffset.Now;

                foreach (var (file, _, size) in entries.Where(e => now - e.Modified > DiskCacheMaxAge).ToList())
                {
                    try
                    {
                        await file.DeleteAsync();
                        totalBytes -= size;
                    }
                    catch { }
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
                        catch { }
                    }
                }
            }
            catch { }
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

            _strongCache.Clear();
            _lru.Clear();
            _lruNodes.Clear();
            _strongCacheBytes = 0;
        }
    }
}
