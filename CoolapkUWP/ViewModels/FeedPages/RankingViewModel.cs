using CoolapkUWP.Controls.DataTemplates;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    /// <summary>
    /// 排行榜页数据源：负责榜单选择条(selectorLinkCard)与各榜单产品列表的加载。
    /// 顶部榜单条默认选中「手机榜」(由服务器 selectedTab 指定)，切换榜单时加载对应的产品列表。
    /// </summary>
    public partial class RankingViewModel : EntityItemSource, IViewModel
    {
        private const string PageName = "V10_CHANNEL_SMB_TOP";

        /// <summary>各榜单对应的产品列表数据源，与 <see cref="Tabs"/> 一一对应。</summary>
        private List<FeedListItemSource> _sources = new List<FeedListItemSource>();

        private string title;
        public string Title
        {
            get => title;
            protected set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private List<RankingTabModel> tabs;
        public List<RankingTabModel> Tabs
        {
            get => tabs;
            protected set
            {
                if (tabs != value)
                {
                    tabs = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private FeedListItemSource selectedSource;
        public FeedListItemSource SelectedSource
        {
            get => selectedSource;
            protected set
            {
                if (selectedSource != value)
                {
                    selectedSource = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool HasTabs => Tabs != null && Tabs.Count > 0;

        public int SelectedIndex { get; private set; } = -1;

        public RankingViewModel() : base(PageName) { }

        /// <summary>
        /// 刷新页面：首次或重置时重新拉取页面卡片数据并重建榜单条，随后刷新当前榜单的产品列表。
        /// </summary>
        public override async Task Refresh(bool reset = false)
        {
            if (!HasTabs || reset)
            {
                (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.DataList, PageName, "&page=1"), true);
                if (isSucceed)
                {
                    BuildTabs(result.AsArray());
                }
            }
            if (SelectedSource != null)
            {
                await SelectedSource.Refresh(reset);
            }
        }

        /// <summary>
        /// 切换到指定榜单，并重新加载其产品列表。
        /// </summary>
        public async Task SelectTabAsync(int index)
        {
            if (!HasTabs || index < 0 || index >= Tabs.Count || index == SelectedIndex) { return; }

            SelectedIndex = index;
            for (int i = 0; i < Tabs.Count; i++)
            {
                Tabs[i].IsSelected = i == index;
            }
            SelectedSource = _sources[index];
            await SelectedSource.Refresh(true);
        }

        /// <summary>
        /// 解析页面卡片数据：selectorLinkCard 生成榜单切换项，configCard 提供页面标题。
        /// </summary>
        private void BuildTabs(JsonArray array)
        {
            try
            {
                List<RankingTabModel> tabs = new List<RankingTabModel>();
                List<FeedListItemSource> sources = new List<FeedListItemSource>();
                string selectedTabName = null;

                foreach (JsonNode node in array)
                {
                    JsonObject json = node.AsObject();
                    switch ((string)json["entityTemplate"])
                    {
                        case "selectorLinkCard":
                            selectedTabName = GetSelectedTab(json);
                            if (json["entities"] is JsonArray entities)
                            {
                                foreach (JsonNode entityNode in entities)
                                {
                                    JsonObject entity = entityNode.AsObject();
                                    string title = (string)entity["title"];
                                    string url = (string)entity["url"];
                                    if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(url)) { continue; }

                                    string rightTopField;
                                    string rightBottomText;
                                    ParseRightConfig(url, out rightTopField, out rightBottomText);

                                    FeedListItemSource source = new FeedListItemSource(title, new CoolapkListProvider(
                                        (p, firstItem, lastItem) => UriHelper.GetUri(
                                            UriType.DataList,
                                            url.Replace("#", "%23").Replace("/", "%2F").Replace("?", "%3F").Replace("=", "%3D").Replace("&", "%26"),
                                            $"&page={p}" + UriHelper.GetOptionalArg("firstItem", firstItem) + UriHelper.GetOptionalArg("lastItem", lastItem)),
                                        (array) => GetRankingEntities(array, rightTopField, rightBottomText),
                                        "id"));
                                    sources.Add(source);
                                    tabs.Add(new RankingTabModel(title, url, rightTopField, rightBottomText));
                                }
                            }
                            break;
                        case "configCard":
                            string pageTitle = GetPageTitle(json);
                            if (!string.IsNullOrEmpty(pageTitle)) { Title = pageTitle; }
                            break;
                    }
                }

                if (tabs.Count == 0) { return; }

                SelectedIndex = tabs.FindIndex(tab => tab.Title == selectedTabName);
                if (SelectedIndex < 0) { SelectedIndex = 0; }
                for (int i = 0; i < tabs.Count; i++)
                {
                    tabs[i].IsSelected = i == SelectedIndex;
                }

                Tabs = tabs;
                _sources = sources;
                SelectedSource = sources[SelectedIndex];
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(RankingViewModel)).LogWarning(ex, ex.ExceptionToMessage());
            }
        }

        /// <summary>
        /// 读取 selectorLinkCard 的 extraDataArr.selectedTab 作为默认选中的榜单名。
        /// </summary>
        private static string GetSelectedTab(JsonObject json)
        {
            if (json["extraDataArr"] is JsonObject extraDataArr)
            {
                string selected = (string)extraDataArr["selectedTab"];
                if (!string.IsNullOrEmpty(selected)) { return selected; }
            }
            return TryReadExtraData(json, "selectedTab");
        }

        /// <summary>
        /// 读取 configCard 的 extraData.pageTitle 作为页面标题。
        /// </summary>
        private static string GetPageTitle(JsonObject json)
        {
            if (json["extraDataArr"] is JsonObject extraDataArr)
            {
                string pageTitle = (string)extraDataArr["pageTitle"];
                if (!string.IsNullOrEmpty(pageTitle)) { return pageTitle; }
            }
            return TryReadExtraData(json, "pageTitle");
        }

        private static string TryReadExtraData(JsonObject json, string key)
        {
            try
            {
                string extraData = (string)json["extraData"];
                if (!string.IsNullOrEmpty(extraData))
                {
                    return (string)JsonNode.Parse(extraData)?[key];
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(RankingViewModel)).LogWarning(ex, ex.ExceptionToMessage());
            }
            return null;
        }

        private IEnumerable<Entity> GetRankingEntities(JsonArray array, string rightTopField, string rightBottomText)
        {
            foreach (JsonNode item in array)
            {
                if (EntityTemplateSelector.GetEntity(item.AsObject()) is Entity entity && entity is IStarRating)
                {
                    if (entity is ProductModel product)
                    {
                        product.SetRankingRight(rightTopField, rightBottomText);
                    }
                    yield return entity;
                }
            }
        }

        /// <summary>
        /// 从榜单 URL 解析右侧分数榜配置：rightTopField(分数字段)与 rightBottomText(下方标签)。
        /// </summary>
        private static void ParseRightConfig(string url, out string rightTopField, out string rightBottomText)
        {
            rightTopField = null;
            rightBottomText = null;
            int queryIndex = url.IndexOf('?');
            if (queryIndex < 0) { return; }
            foreach (string pair in url.Substring(queryIndex + 1).Split('&'))
            {
                int equalsIndex = pair.IndexOf('=');
                if (equalsIndex < 0) { continue; }
                string key = Uri.UnescapeDataString(pair.Substring(0, equalsIndex));
                string value = Uri.UnescapeDataString(pair.Substring(equalsIndex + 1));
                if (key == "rightTopField") { rightTopField = value; }
                else if (key == "rightBottomText") { rightBottomText = value; }
            }
        }

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(name)); }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is RankingViewModel model && IsEqual(model);

        public bool IsEqual(RankingViewModel other) => other != null;
    }
}