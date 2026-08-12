using CoolapkUWP.ViewModels.FeedPages;
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
    public sealed partial class FindPage : PivotPageBase
    {
        public FindPage() => InitializeComponent();

        protected override Pivot PivotControl => Pivot;

        protected override ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FindPage");
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem() { Tag = "V11_HOME_NEW", Header = loader.GetString("V11_HOME_NEW"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_SHIPIN", Header = loader.GetString("V9_HOME_TAB_SHIPIN"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_CAR", Header = loader.GetString("V11_HOME_CAR"), Content = new Frame() },
                new PivotItem() { Tag = "V10_DIGITAL_HOME", Header = loader.GetString("V10_DIGITAL_HOME"), Content = new Frame() },
                new PivotItem() { Tag = "V10_CHANNEL_SJB", Header = loader.GetString("V10_CHANNEL_SJB"), Content = new Frame() },
                new PivotItem() { Tag = "V11_ZHUANTI_EARPHONE", Header = loader.GetString("V11_ZHUANTI_EARPHONE"), Content = new Frame() },
                new PivotItem() { Tag = "V11_FIND_GOOD_GOODS_HOME", Header = loader.GetString("V11_FIND_GOOD_GOODS_HOME"), Content = new Frame() },
            };
            return items;
        }

        protected override void NavigateToPage(PivotItem item, Frame frame)
        {
            string url = item.Tag.ToString() == "V9_HOME_TAB_HEADLINE"
                ? "/main/indexV8"
                : item.Tag.ToString() == "V11_FIND_DYH"
                    ? "/user/dyhSubscribe"
                    : $"/page?url={item.Tag}";
            _ = frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
        }
    }
}
