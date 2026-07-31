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
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Common
{
    public class ImageCache
    {
        private const int MemoryCacheMaxCount = 150;
        private const long MaxCacheableImageBytes = 2 * 1024 * 1024;
        private const int MaxDecodePixelWidth = 1280;
        private const long DiskCacheMaxBytes = 512L * 1024 * 1024;
        private static readonly TimeSpan DiskCacheMaxAge = TimeSpan.FromDays(30);

        private static readonly Lazy<ImageCache> _instance = new Lazy<ImageCache>(() => new ImageCache());

        public static ImageCache Instance => _instance.Value;

        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromDays(7);

        private Task<StorageFolder> _cacheFolderTask;
        private Task _maintainTask;
        private readonly HttpClient _httpClient;

        private readonly Dictionary<string, WeakReference<BitmapImage>> _memoryCache = new Dictionary<string, WeakReference<BitmapImage>>();

        private readonly ConcurrentDictionary<string, Task<BitmapImage>> _inflightDecodes = new ConcurrentDictionary<string, Task<BitmapImage>>();
        private readonly ConcurrentDictionary<string, Lazy<Task<StorageFile>>> _inflightDownloads = new ConcurrentDictionary<string, Lazy<Task<StorageFile>>>();

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

            if (decodePixelWidth <= 0 || decodePixelWidth > MaxDecodePixelWidth) { decodePixelWidth = MaxDecodePixelWidth; }

            string fileName = GetCacheFileName(uri);
            string key = GetCacheKey(fileName, decodePixelWidth);

            BitmapImage cached = GetFromMemoryCache(key);
            if (cached == null) { cached = GetBestFromMemoryCache(fileName, decodePixelWidth); }
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
            if (bitmap != null && CanCache(bitmap))
            {
                AddToMemoryCache(key, bitmap);
            }
            return bitmap;
        }

        private static bool CanCache(BitmapImage bitmap)
        {
            long size = EstimateBytes(bitmap);
            return size > 0 && size <= MaxCacheableImageBytes;
        }

        private static async Task<BitmapImage> DecodeImageAsync(StorageFile file, int decodePixelWidth)
        {
            await _decodeSemaphore.WaitAsync();
            try
            {
                try
                {
                    var bitmap = new BitmapImage();
                    if (decodePixelWidth > 0) { bitmap.DecodePixelWidth = decodePixelWidth; }
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

        private BitmapImage GetFromMemoryCache(string key)
        {
            if (_memoryCache.TryGetValue(key, out WeakReference<BitmapImage> weakRef))
            {
                if (weakRef.TryGetTarget(out BitmapImage bitmap)) { return bitmap; }
                _memoryCache.Remove(key);
            }
            return null;
        }

        private BitmapImage GetBestFromMemoryCache(string fileName, int decodePixelWidth)
        {
            string bestKey = null;
            int bestWidth = int.MaxValue;
            List<string> dead = null;
            foreach (KeyValuePair<string, WeakReference<BitmapImage>> entry in _memoryCache)
            {
                if (!entry.Key.StartsWith(fileName + "|", StringComparison.Ordinal)) { continue; }
                if (!int.TryParse(entry.Key.Substring(fileName.Length + 1), out int width)) { continue; }
                if (width < decodePixelWidth || width >= bestWidth) { continue; }
                if (entry.Value.TryGetTarget(out _))
                {
                    bestKey = entry.Key;
                    bestWidth = width;
                }
                else
                {
                    (dead ??= new List<string>()).Add(entry.Key);
                }
            }

            if (dead != null)
            {
                foreach (string key in dead) { _memoryCache.Remove(key); }
            }
            if (bestKey == null) { return null; }
            return _memoryCache[bestKey].TryGetTarget(out BitmapImage best) ? best : null;
        }

        private void AddToMemoryCache(string key, BitmapImage bitmap)
        {
            if (_memoryCache.ContainsKey(key)) { return; }

            long size = EstimateBytes(bitmap);
            if (size <= 0) { return; }

            _memoryCache[key] = new WeakReference<BitmapImage>(bitmap);

            if (_memoryCache.Count > MemoryCacheMaxCount)
            {
                PruneMemoryCache();
            }
        }

        private void PruneMemoryCache()
        {
            List<string> dead = null;
            foreach (KeyValuePair<string, WeakReference<BitmapImage>> entry in _memoryCache)
            {
                if (!entry.Value.TryGetTarget(out _))
                {
                    (dead ??= new List<string>()).Add(entry.Key);
                }
            }
            if (dead != null)
            {
                foreach (string key in dead) { _memoryCache.Remove(key); }
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
            Lazy<Task<StorageFile>> lazy = _inflightDownloads.GetOrAdd(fileName, _ => new Lazy<Task<StorageFile>>(() => DownloadToFileCoreAsync(uri, folder, fileName)));
            try
            {
                return await lazy.Value;
            }
            finally
            {
                _inflightDownloads.TryRemove(new KeyValuePair<string, Lazy<Task<StorageFile>>>(fileName, lazy));
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

            _memoryCache.Clear();
        }
    }
}
