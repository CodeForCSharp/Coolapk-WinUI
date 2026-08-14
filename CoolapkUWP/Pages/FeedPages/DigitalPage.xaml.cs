using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了"空白页"项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 数码页：对应酷安客户端的「数码」Tab，聚合数码库/数码/手机/排行榜/系统/电脑。
    /// </summary>
    public sealed partial class DigitalPage : PivotPageBase
    {
        public DigitalPage() => InitializeComponent();

        protected override Pivot PivotControl => Pivot;

        protected override ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("DigitalPage");
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem() { Tag = "/product/categoryList", Header = loader.GetString("DigitalLibrary"), Content = new Frame() },
                new PivotItem() { Tag = "V10_DIGITAL_HOME", Header = loader.GetString("DigitalHome"), Content = new Frame() },
                new PivotItem() { Tag = "V10_CHANNEL_SJB", Header = loader.GetString("Phone"), Content = new Frame() },
                new PivotItem() { Tag = "V10_CHANNEL_SMB_TOP", Header = loader.GetString("Ranking"), Content = new Frame() },
                new PivotItem() { Tag = "V13_DIGITAL_ROM", Header = loader.GetString("Rom"), Content = new Frame() },
                new PivotItem() { Tag = "V8_ZHUANTI_COMPUTER_20230413", Header = loader.GetString("Computer"), Content = new Frame() },
            };
            return items;
        }

        protected override void NavigateToPage(PivotItem item, Frame frame)
        {
            string tag = item.Tag.ToString();
            if (tag == "/product/categoryList")
            {
                _ = frame.Navigate(typeof(DigitalLibraryPage));
            }
            else
            {
                string url = tag.StartsWith("/", StringComparison.Ordinal) ? tag : $"/page?url={tag}";
                _ = frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
            }
        }
    }
}
