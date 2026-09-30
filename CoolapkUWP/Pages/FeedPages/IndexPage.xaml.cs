using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.FeedPages;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 首页：对应酷安客户端的「首页」Tab，栏目由 /v6/main/init 的首页配置卡下发。
    /// </summary>
    public sealed partial class IndexPage : PivotPageBase
    {
        private const string HeadlineTab = "V9_HOME_TAB_HEADLINE";
        private const string TopicTab = "V11_VERTICAL_TOPIC";

        public IndexPage() => InitializeComponent();

        protected override Pivot PivotControl => Pivot;

        protected override async Task<ObservableCollection<PivotItem>> GetMainItemsAsync()
        {
            ObservableCollection<PivotItem> items = await GetRemoteItemsAsync();
            return items.Count > 0 ? items : GetMainItems();
        }

        /// <summary>
        /// 接口失败或没有首页配置卡时使用的本地栏目。
        /// </summary>
        protected override ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("IndexPage");
            string[] tags =
            {
                "V9_HOME_TAB_HEADLINE", "V9_HOME_TAB_FOLLOW", "V9_HOME_TAB_RANKING", "V11_HOME_TAB_NEWS",
                "V11_VERTICAL_TOPIC", "V11_HOME_NEW", "V13_IOSHOME_OPENSHOW", "V13_HOME_SHEYING",
                "V11_HOME_TAB_JC", "V11_HOME_CAR", "V9_HOME_TAB_SHIPIN", "V11_HOME_MEIHUA",
                "V9_HOME_TAB_LIVE", "V9_HOME_TAB_WENDA",
            };
            return new ObservableCollection<PivotItem>(tags.Select(tag => CreateItem(tag, loader.GetString(tag))));
        }

        protected override int GetDefaultIndex(ObservableCollection<PivotItem> items)
        {
            PivotItem headline = items.FirstOrDefault(item => IsHeadline(item.Tag as string));
            return headline == null ? 0 : items.IndexOf(headline);
        }

        protected override void NavigateToPage(PivotItem item, Frame frame)
        {
            string tag = item.Tag.ToString();
            if (tag == TopicTab)
            {
                _ = frame.Navigate(typeof(TopicColumnsPage));
            }
            else
            {
                string url = IsHeadline(tag) ? "/main/indexV8" : IsPageName(tag) ? $"/page?url={tag}" : tag;
                _ = frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
            }
        }

        /// <summary>
        /// 首页配置卡：entityId 为 6390，或模板为 configCard 且标题为「首页」或包含「TAB配置」。
        /// 栏目优先用 page_name 标识，没有时用 url。
        /// </summary>
        private static async Task<ObservableCollection<PivotItem>> GetRemoteItemsAsync()
        {
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>();
            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetIndexPageNames), true);
            if (!isSucceed || result is not JsonArray array) { return items; }

            JsonObject card = array.OfType<JsonObject>().FirstOrDefault(IsHomeConfigCard);
            if (card?["entities"] is not JsonArray entities) { return items; }

            foreach (JsonObject entity in entities.OfType<JsonObject>())
            {
                string title = (string)entity["title"];
                string pageName = (string)entity["page_name"];
                string key = string.IsNullOrEmpty(pageName) ? (string)entity["url"] : pageName;
                if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(key))
                {
                    items.Add(CreateItem(key, title));
                }
            }
            return items;
        }

        private static bool IsHomeConfigCard(JsonObject json)
        {
            if (json["entityId"]?.ToString() == "6390") { return true; }
            if ((string)json["entityTemplate"] != "configCard") { return false; }
            string title = (string)json["title"] ?? string.Empty;
            return title == "首页" || title.Contains("TAB配置");
        }

        private static PivotItem CreateItem(string tag, string header) => new PivotItem() { Tag = tag, Header = header, Content = new Frame() };

        private static bool IsHeadline(string tag) => tag == HeadlineTab || tag == "/main/headline";

        private static bool IsPageName(string tag) => !string.IsNullOrEmpty(tag) && !tag.Contains('/') && !tag.Contains('#');
    }
}
