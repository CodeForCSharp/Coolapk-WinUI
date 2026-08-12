using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Upload;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;


namespace CoolapkUWP.ViewModels.FeedPages
{
    public partial class CreateFeedViewModel : ObservableObject, IViewModel
    {
        public static string[] ImageTypes = new string[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif", ".heif", ".heic" };

        [ObservableProperty]
        public partial string Title { get; set; }

        public readonly CreateUserItemSource CreateUserItemSource = new CreateUserItemSource();
        public readonly CreateTopicItemSource CreateTopicItemSource = new CreateTopicItemSource();

        public readonly ObservableCollection<WriteableBitmap> Pictures = new ObservableCollection<WriteableBitmap>();

        public CreateFeedViewModel() { }

        public async Task Refresh(bool reset)
        {
            await CreateUserItemSource.Refresh(reset);
            await CreateTopicItemSource.Refresh(reset);
        }

        bool IViewModel.IsEqual(IViewModel other) => other is CreateFeedViewModel model && Equals(model);

        public async Task ReadFile(IStorageFile file)
        {
            using (IRandomAccessStreamWithContentType stream = await file.OpenReadAsync())
            {
                await ReadStream(stream);
            }
        }

        public async Task ReadStream(IRandomAccessStream stream)
        {
            BitmapDecoder ImageDecoder = await BitmapDecoder.CreateAsync(stream);
            SoftwareBitmap SoftwareImage = await ImageDecoder.GetSoftwareBitmapAsync();
            try
            {
                WriteableBitmap WriteableImage = new WriteableBitmap((int)ImageDecoder.PixelWidth, (int)ImageDecoder.PixelHeight);
                await WriteableImage.SetSourceAsync(stream);
                Pictures.Add(WriteableImage);
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(CreateFeedViewModel)).LogWarning(ex, ex.ExceptionToMessage());
                try
                {
                    using (InMemoryRandomAccessStream random = new InMemoryRandomAccessStream())
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, random);
                        encoder.SetSoftwareBitmap(SoftwareImage);
                        await encoder.FlushAsync();
                        WriteableBitmap WriteableImage = new WriteableBitmap((int)ImageDecoder.PixelWidth, (int)ImageDecoder.PixelHeight);
                        await WriteableImage.SetSourceAsync(random);
                        Pictures.Add(WriteableImage);
                    }
                }
                catch (Exception e)
                {
                    SettingsHelper.LogManager.CreateLogger(nameof(CreateFeedViewModel)).LogError(e, e.ExceptionToMessage());
                }
            }
        }

        public async Task<bool> CheckData(DataPackageView data)
        {
            if (data.Contains(StandardDataFormats.Bitmap))
            {
                return true;
            }
            else if (data.Contains(StandardDataFormats.StorageItems))
            {
                IReadOnlyList<IStorageItem> items = await data.GetStorageItemsAsync();
                IEnumerable<IStorageItem> images = items.Where(i => i is StorageFile).Where(i =>
                {
                    foreach (string type in ImageTypes)
                    {
                        if (i.Name.EndsWith(type, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    return false;
                });
                if (images.Any()) { return true; }
            }
            return false;
        }

        public async void PickImage()
        {
            FileOpenPicker FileOpen = new FileOpenPicker();
            ((IInitializeWithWindow)(object)FileOpen).Initialize(App.WindowHandle);
            FileOpen.FileTypeFilter.Add(".jpg");
            FileOpen.FileTypeFilter.Add(".jpeg");
            FileOpen.FileTypeFilter.Add(".png");
            FileOpen.FileTypeFilter.Add(".bmp");
            FileOpen.SuggestedStartLocation = PickerLocationId.ComputerFolder;

            foreach (StorageFile file in await FileOpen.PickMultipleFilesAsync())
            {
                if (file != null) { await ReadFile(file); }
            }
        }

        public async Task<IList<string>> UploadPic()
        {
            IList<string> results = new List<string>();
            if (!Pictures.Any()) { return results; }
            UIHelper.ShowMessage("上传图片");
            List<UploadFileFragment> fragments = new List<UploadFileFragment>();
            foreach (WriteableBitmap pic in Pictures)
            {
                fragments.Add(await UploadFileFragment.FromWriteableBitmap(pic));
            }
            results = await RequestHelper.UploadImages(fragments);
            UIHelper.ShowMessage($"上传了 {results.Count} 张图片");
            return results;
        }

        public async Task DropFile(DataPackageView data)
        {
            if (data.Contains(StandardDataFormats.Bitmap))
            {
                RandomAccessStreamReference bitmap = await data.GetBitmapAsync();
                using (IRandomAccessStreamWithContentType random = await bitmap.OpenReadAsync())
                {
                    await ReadStream(random);
                }
            }
            else if (data.Contains(StandardDataFormats.StorageItems))
            {
                IReadOnlyList<IStorageItem> items = await data.GetStorageItemsAsync();
                IEnumerable<IStorageItem> images = items.Where(i => i is StorageFile).Where(i =>
                {
                    foreach (string type in ImageTypes)
                    {
                        if (i.Name.EndsWith(type, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    return false;
                });
                if (images.Any()) { await ReadFile(images.FirstOrDefault() as StorageFile); }
            }
        }
    }

    public partial class CreateUserItemSource : KeywordSearchItemSource
    {
        public CreateUserItemSource(string keyword = " ") : base(keyword) { }

        protected override void UpdateProvider()
        {
            if (!string.IsNullOrWhiteSpace(Keyword))
            {
                Provider = new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                    UriHelper.GetUri(
                        UriType.SearchCreateUsers,
                        Keyword,
                        p,
                        UriHelper.GetPagingArgs(p, firstItem, lastItem)),
                    GetEntities,
                    "uid");
            }
            else if (SettingsHelper.Get<string>(SettingsHelper.Uid) is string uid && !string.IsNullOrEmpty(uid))
            {
                Provider = new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetUserList,
                            "followList",
                            uid,
                            p,
                            UriHelper.GetOptionalArg("firstItem", firstItem),
                            UriHelper.GetOptionalArg("lastItem", lastItem)),
                    o => new[] { UserModel.FromJson(o["fUserInfo"].AsObject()) },
                    "fuid");
            }
        }

        private IEnumerable<Entity> GetEntities(JsonObject jo) => new[] { UserModel.FromJson(jo) };
    }

    public partial class CreateTopicItemSource : KeywordSearchItemSource
    {
        public CreateTopicItemSource(string keyword = " ") : base(keyword) { }

        protected override void UpdateProvider()
        {
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.SearchCreateTags,
                    Keyword,
                    p,
                    UriHelper.GetPagingArgs(p, firstItem, lastItem)),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JsonObject jo) => new[] { TopicModel.FromJson(jo) };
    }
}
