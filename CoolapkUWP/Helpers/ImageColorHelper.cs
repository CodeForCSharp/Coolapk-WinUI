using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;

namespace CoolapkUWP.Helpers
{
    /// <summary>
    /// 从图片提取主色调,结果按 URL 缓存复用。
    /// </summary>
    internal static class ImageColorHelper
    {
        private const uint MaxSide = 32;
        private const int BucketLevels = 32;

        private static readonly ConcurrentDictionary<string, Task<Color?>> Cache = new();

        internal static Task<Color?> GetDominantColorAsync(string url)
        {
            if (string.IsNullOrEmpty(url)) { return Task.FromResult<Color?>(null); }
            return Cache.GetOrAdd(url, static key => ComputeAsync(key));
        }

        private static async Task<Color?> ComputeAsync(string url)
        {
            try
            {
                StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.Icon, url);
                if (file == null) { return null; }

                using (IRandomAccessStream stream = await file.OpenReadAsync())
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    uint scale = Math.Max(decoder.PixelWidth, decoder.PixelHeight);
                    if (scale == 0) { return null; }

                    double ratio = scale > MaxSide ? (double)MaxSide / scale : 1.0;
                    BitmapTransform transform = new BitmapTransform
                    {
                        ScaledWidth = Math.Max(1, (uint)Math.Round(decoder.PixelWidth * ratio)),
                        ScaledHeight = Math.Max(1, (uint)Math.Round(decoder.PixelHeight * ratio)),
                        InterpolationMode = BitmapInterpolationMode.Linear,
                    };

                    SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Premultiplied,
                        transform,
                        ExifOrientationMode.RespectExifOrientation,
                        ColorManagementMode.DoNotColorManage);

                    return ComputeDominant(bitmap);
                }
            }
            catch (Exception)
            {
                _ = Cache.TryRemove(url, out _);
                return null;
            }
        }

        private static Color? ComputeDominant(SoftwareBitmap bitmap)
        {
            int width = (int)bitmap.PixelWidth;
            int height = (int)bitmap.PixelHeight;
            if (width <= 0 || height <= 0) { return null; }

            byte[] pixels = new byte[width * height * 4];
            bitmap.CopyToBuffer(pixels.AsBuffer());

            int bucketCount = BucketLevels * BucketLevels * BucketLevels;
            int[] counts = new int[bucketCount];
            long[] sumR = new long[bucketCount];
            long[] sumG = new long[bucketCount];
            long[] sumB = new long[bucketCount];

            for (int i = 0; i < pixels.Length; i += 4)
            {
                byte b = pixels[i];
                byte g = pixels[i + 1];
                byte r = pixels[i + 2];
                byte a = pixels[i + 3];
                if (a < 128) { continue; }

                int lum = (r * 299 + g * 587 + b * 114) / 1000;
                if (lum < 24 || lum > 235) { continue; }

                int idx = ((r >> 3) * BucketLevels + (g >> 3)) * BucketLevels + (b >> 3);
                counts[idx]++;
                sumR[idx] += r;
                sumG[idx] += g;
                sumB[idx] += b;
            }

            int best = -1;
            int bestCount = 0;
            for (int i = 0; i < bucketCount; i++)
            {
                if (counts[i] > bestCount)
                {
                    bestCount = counts[i];
                    best = i;
                }
            }
            if (best < 0) { return null; }

            int avgR = (int)(sumR[best] / counts[best]);
            int avgG = (int)(sumG[best] / counts[best]);
            int avgB = (int)(sumB[best] / counts[best]);

            double lumD = (avgR * 299 + avgG * 587 + avgB * 114) / 1000.0;
            if (lumD <= 0) { return null; }

            const double SaturationFactor = 1.35;
            avgR = (int)Math.Min(255, Math.Max(0, lumD + (avgR - lumD) * SaturationFactor));
            avgG = (int)Math.Min(255, Math.Max(0, lumD + (avgG - lumD) * SaturationFactor));
            avgB = (int)Math.Min(255, Math.Max(0, lumD + (avgB - lumD) * SaturationFactor));

            int l = (avgR * 299 + avgG * 587 + avgB * 114) / 1000;
            if (l < 60)
            {
                double k = 60.0 / l;
                avgR = Math.Min(255, (int)(avgR * k));
                avgG = Math.Min(255, (int)(avgG * k));
                avgB = Math.Min(255, (int)(avgB * k));
            }
            else if (l > 220)
            {
                double k = 220.0 / l;
                avgR = (int)(avgR * k);
                avgG = (int)(avgG * k);
                avgB = (int)(avgB * k);
            }

            return Color.FromArgb(255, (byte)avgR, (byte)avgG, (byte)avgB);
        }
    }
}
