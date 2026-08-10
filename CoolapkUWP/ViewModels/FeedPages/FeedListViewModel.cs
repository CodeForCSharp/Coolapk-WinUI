using CoolapkUWP.Controls;
using CoolapkUWP.Controls.DataTemplates;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Dispatching;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public abstract class FeedListViewModel : IViewModel
    {
        protected const string idName = "id";

        public string ID { get; }
        private FeedListType ListType { get; }
        public DataTemplateSelector DataTemplateSelector;

        private string title;
        public string Title
        {
            get => title;
            protected set
            {
                title = value;
                RaisePropertyChangedEvent();
            }
        }

        private List<ShyHeaderItem> itemSource;
        public List<ShyHeaderItem> ItemSource
        {
            get => itemSource;
            protected set
            {
                itemSource = value;
                RaisePropertyChangedEvent();
            }
        }

        private FeedListDetailBase detail;
        public FeedListDetailBase Detail
        {
            get => detail;
            protected set
            {
                detail = value;
                RaisePropertyChangedEvent();
            }
        }

        private DataTemplate detailDataTemplate;
        public DataTemplate DetailDataTemplate
        {
            get => detailDataTemplate;
            protected set
            {
                detailDataTemplate = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedListViewModel(string id, FeedListType type)
        {
            ID = string.IsNullOrEmpty(id)
                ? throw new ArgumentException(nameof(id))
                : id;
            ListType = type;
        }

        public static FeedListViewModel GetProvider(FeedListType type, string id)
        {
            if (string.IsNullOrEmpty(id) || id == "0") { return null; }
            switch (type)
            {
                case FeedListType.UserPageList: return new UserViewModel(id);
                case FeedListType.TagPageList: return new TagViewModel(id);
                case FeedListType.DyhPageList: return new DyhViewModel(id);
                case FeedListType.ProductPageList: return new ProductViewModel(id);
                case FeedListType.CollectionPageList: return new CollectionViewModel(id);
                default: return null;
            }
        }

        public void ChangeCopyMode(bool mode)
        {
            if (Detail != null)
            {
                Detail.IsCopyEnabled = mode;
            }
        }

        public async void CopyPic(ImageModel image)
        {
            DataPackage dataPackage = await GetImageDataPackage(image, "复制图片");
            Clipboard.SetContentWithOptions(dataPackage, null);
        }

        public async void SharePic(ImageModel image)
        {
            await GetImageDataPackage(image, "分享图片");
        }

        public async void SavePic(ImageModel imageModel)
        {
            string url = imageModel.Uri;
            StorageFile image = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, url);
            if (image == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
                return;
            }

            string fileName = GetTitle(url);
            FileSavePicker fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = fileName.Replace(fileName.Substring(fileName.LastIndexOf('.')), string.Empty)
            };
            ((IInitializeWithWindow)(object)fileSavePicker).Initialize(App.WindowHandle);

            string fileex = fileName.Substring(fileName.LastIndexOf('.') + 1);
            int index = fileex.IndexOfAny(new char[] { '?', '%', '&' });
            fileex = fileex.Substring(0, index == -1 ? fileex.Length : index);
            fileSavePicker.FileTypeChoices.Add($"{fileex}文件", new string[] { "." + fileex });

            StorageFile file = await fileSavePicker.PickSaveFileAsync();
            if (file != null)
            {
                using (Stream FolderStream = await file.OpenStreamForWriteAsync())
                {
                    using (IRandomAccessStreamWithContentType RandomAccessStream = await image.OpenReadAsync())
                    {
                        using (Stream ImageStream = RandomAccessStream.AsStreamForRead())
                        {
                            await ImageStream.CopyToAsync(FolderStream);
                        }
                    }
                }
            }
        }

        public async Task<DataPackage> GetImageDataPackage(ImageModel image, string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, image.Uri);
            if (file == null) { return null; }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = GetTitle(image.Uri);

            return dataPackage;
        }

        public async Task GetImageDataPackage(DataPackage dataPackage, ImageModel image, string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, image.Uri);
            if (file == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
                return;
            }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = GetTitle(image.Uri);
            dataPackage.SetStorageItems(new IStorageItem[] { file });
        }

        private string GetTitle(string url)
        {
            Regex regex = new Regex(@"[^/]+(?!.*/)");
            return regex.IsMatch(url) ? regex.Match(url).Value : "图片";
        }
        public abstract Task<FeedListDetailBase> GetDetail();

        public abstract Task Refresh(bool reset = false);

        bool IViewModel.IsEqual(IViewModel other) => other is FeedListViewModel model && IsEqual(model);

        public bool IsEqual(FeedListViewModel other) => ListType == other.ListType && ID == other.ID;

        protected abstract string GetTitleBarText(FeedListDetailBase detail);

        private IEnumerable<Entity> GetEntities(JsonObject jo)
        {
            yield return EntityTemplateSelector.GetEntity(jo);
        }

}
