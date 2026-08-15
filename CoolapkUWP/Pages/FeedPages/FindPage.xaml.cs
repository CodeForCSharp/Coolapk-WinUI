using CoolapkUWP.ViewModels.FeedPages;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了"空白页"项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 发现页：对应酷安客户端的「发现」Tab，聚合酷品/二手/酷图/看看号/好物榜。
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
                new PivotItem() { Tag = "V11_FIND_GOODS", Header = loader.GetString("V11_FIND_GOODS"), Content = new Frame() },
                new PivotItem() { Tag = "V11_DISCOVERY_SECOND_HAND", Header = loader.GetString("V11_DISCOVERY_SECOND_HAND"), Content = new Frame() },
                new PivotItem() { Tag = "V11_FIND_COOLPIC", Header = loader.GetString("V11_FIND_COOLPIC"), Content = new Frame() },
                new PivotItem() { Tag = "V11_FIND_DYH", Header = loader.GetString("V11_FIND_DYH"), Content = new Frame() },
                new PivotItem() { Tag = "V12_FIND_KUBANG", Header = loader.GetString("V12_FIND_KUBANG"), Content = new Frame() },
            };
            return items;
        }

        protected override void NavigateToPage(PivotItem item, Frame frame)
        {
            string url = item.Tag.ToString() == "V11_FIND_DYH"
                ? "/user/dyhSubscribe"
                : $"/page?url={item.Tag}";
            _ = frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
        }
    }
}
