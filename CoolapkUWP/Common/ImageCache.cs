using CoolapkUWP.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Common
{
    public class ImageCache
    {
        [ThreadStatic]
        private static ImageCache _instance;

        public static ImageCache Instance => _instance ?? (_instance = new ImageCache());

        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromDays(7);

        private StorageFolder _cacheFolder;
        private HttpClient _httpClient;

        static ImageCache() { }

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

        private async Task<StorageFolder> GetCacheFolderAsync()
        {
            if (_cacheFolder == null)
            {
                _cacheFolder = await ApplicationData.Current.LocalCacheFolder.CreateFolderAsync("ImageCache", CreationCollisionOption.OpenIfExists);
            }
            return _cacheFolder;
        }

        private static string GetCacheFileName(Uri uri)
        {
            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(uri.ToString()));
            return Convert.ToHexString(hash);
        }

        public async Task<BitmapImage> GetFromCacheAsync(Uri uri, bool throwOnFailure = false)
        {
            try
            {
                var folder = await GetCacheFolderAsync();
                string fileName = GetCacheFileName(uri);
                var file = await folder.TryGetItemAsync(fileName) as StorageFile;

                if (file != null)
                {
                    var props = await file.GetBasicPropertiesAsync();
                    if (DateTimeOffset.Now - props.DateModified < CacheDuration)
                    {
                        var bitmap = await DecodeImageAsync(file);
                        if (bitmap != null) { return bitmap; }
                        await file.DeleteAsync();
                    }
                }

                file = await DownloadToFileAsync(uri, folder, fileName);
                var result = await DecodeImageAsync(file);
                if (result != null) { return result; }
                await file.DeleteAsync();
            }
            catch { }
            return null;
        }

        private static async Task<BitmapImage> DecodeImageAsync(StorageFile file)
        {
            try
            {
                var bitmap = new BitmapImage();
                using (var stream = await file.OpenReadAsync())
                {
                    await bitmap.SetSourceAsync(stream);
                }
                return bitmap;
            }
            catch { return null; }
        }

        private async Task<StorageFile> DownloadToFileAsync(Uri uri, StorageFolder folder, string fileName)
        {
            byte[] data = await _httpClient.GetByteArrayAsync(uri);
            StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(file, data);
            return file;
        }

        public async Task<StorageFile> GetFileFromCacheAsync(Uri uri)
        {
            var folder = await GetCacheFolderAsync();
            string fileName = GetCacheFileName(uri);
            var file = await folder.TryGetItemAsync(fileName) as StorageFile;

            if (file != null)
            {
                var props = await file.GetBasicPropertiesAsync();
                if (DateTimeOffset.Now - props.DateModified < CacheDuration)
                    return file;
            }

            return await DownloadToFileAsync(uri, folder, fileName);
        }

        public async Task RemoveAsync(Uri[] uris)
        {
            var folder = await GetCacheFolderAsync();
            foreach (var uri in uris)
            {
                string fileName = GetCacheFileName(uri);
                var file = await folder.TryGetItemAsync(fileName) as StorageFile;
                if (file != null)
                    await file.DeleteAsync();
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
        }
    }
}
