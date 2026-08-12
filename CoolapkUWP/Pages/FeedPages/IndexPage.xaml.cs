using CoolapkUWP.ViewModels.FeedPages;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IndexPage : PivotPageBase
    {
        public IndexPage() => InitializeComponent();

        protected override Pivot PivotControl => Pivot;

        protected override ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("IndexPage");
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem() { Tag = "V9_HOME_TAB_HEADLINE", Header = loader.GetString("V9_HOME_TAB_HEADLINE"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_WENDA", Header = loader.GetString("V9_HOME_TAB_WENDA"), Content = new Frame() },
                new PivotItem() { Tag = "V11_FIND_COOLPIC", Header = loader.GetString("V11_FIND_COOLPIC"), Content = new Frame() },
                new PivotItem() { Tag = "V11_FIND_DYH", Header = loader.GetString("V11_FIND_DYH"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_RANKING", Header = loader.GetString("V9_HOME_TAB_RANKING"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_TAB_NEWS", Header = loader.GetString("V11_HOME_TAB_NEWS"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_TAB_JC", Header = loader.GetString("V11_HOME_TAB_JC"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_MEIHUA", Header = loader.GetString("V11_HOME_MEIHUA"), Content = new Frame() },
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
