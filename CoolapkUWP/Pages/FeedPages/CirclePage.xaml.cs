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
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CirclePage : PivotPageBase
    {
        public CirclePage() => InitializeComponent();

        protected override Pivot PivotControl => Pivot;

        protected override ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("CirclePage");
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem() { Tag = "V9_HOME_TAB_FOLLOW", Header = loader.GetString("V9_HOME_TAB_FOLLOW"), Content = new Frame() },
                new PivotItem() { Tag = "circle", Header = loader.GetString("circle"), Content = new Frame() },
                new PivotItem() { Tag = "apk", Header = loader.GetString("apk"), Content = new Frame() },
                new PivotItem() { Tag = "topic", Header = loader.GetString("topic"), Content = new Frame() },
                new PivotItem() { Tag = "question", Header = loader.GetString("question"), Content = new Frame() },
                new PivotItem() { Tag = "product", Header = loader.GetString("product"), Content = new Frame() }
            };
            return items;
        }

        protected override void NavigateToPage(PivotItem item, Frame frame)
        {
            _ = frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(item.Tag.ToString().Contains("V") ? $"/page?url={item.Tag}" : $"/page?url=V9_HOME_TAB_FOLLOW&type={item.Tag}"));
        }
    }
}
