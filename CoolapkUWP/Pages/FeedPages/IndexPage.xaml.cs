using CoolapkUWP.ViewModels.FeedPages;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.Resources;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 首页：对应酷安客户端的「首页」Tab，聚合关注/头条/热榜等 14 个子 Tab。
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
                new PivotItem() { Tag = "V9_HOME_TAB_FOLLOW", Header = loader.GetString("V9_HOME_TAB_FOLLOW"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_RANKING", Header = loader.GetString("V9_HOME_TAB_RANKING"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_TAB_NEWS", Header = loader.GetString("V11_HOME_TAB_NEWS"), Content = new Frame() },
                new PivotItem() { Tag = "V11_VERTICAL_TOPIC", Header = loader.GetString("V11_VERTICAL_TOPIC"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_NEW", Header = loader.GetString("V11_HOME_NEW"), Content = new Frame() },
                new PivotItem() { Tag = "V13_IOSHOME_OPENSHOW", Header = loader.GetString("V13_IOSHOME_OPENSHOW"), Content = new Frame() },
                new PivotItem() { Tag = "V13_HOME_SHEYING", Header = loader.GetString("V13_HOME_SHEYING"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_TAB_JC", Header = loader.GetString("V11_HOME_TAB_JC"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_CAR", Header = loader.GetString("V11_HOME_CAR"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_SHIPIN", Header = loader.GetString("V9_HOME_TAB_SHIPIN"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_MEIHUA", Header = loader.GetString("V11_HOME_MEIHUA"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_LIVE", Header = loader.GetString("V9_HOME_TAB_LIVE"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_WENDA", Header = loader.GetString("V9_HOME_TAB_WENDA"), Content = new Frame() },
            };
            return items;
        }

        protected override void NavigateToPage(PivotItem item, Frame frame)
        {
            string tag = item.Tag.ToString();
            if (tag == "V11_VERTICAL_TOPIC")
            {
                _ = frame.Navigate(typeof(TopicColumnsPage));
            }
            else
            {
                string url = tag == "V9_HOME_TAB_HEADLINE" ? "/main/indexV8" : $"/page?url={tag}";
                _ = frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
            }
        }
    }
}
