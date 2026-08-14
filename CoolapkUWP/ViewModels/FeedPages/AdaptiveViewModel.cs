using CoolapkUWP.Controls;
using CoolapkUWP.Controls.DataTemplates;
using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public partial class AdaptiveViewModel : EntityItemSource, IViewModel
    {
        private readonly string Uri;
        protected bool IsInitPage => Uri == "/main/init";
        protected bool IsIndexPage => !Uri.Contains("?");
        protected bool IsHotFeedPage => Uri == "/main/indexV8" || Uri == "/main/index";

        private string title = string.Empty;
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

        private bool isShowTitle;
        public bool IsShowTitle
        {
            get => isShowTitle;
            set
            {
                if (isShowTitle != value)
                {
                    isShowTitle = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private List<ShyHeaderItem> tabs;
        public List<ShyHeaderItem> Tabs
        {
            get => tabs;
            private set
            {
                if (tabs != value)
                {
                    tabs = value;
                    RaisePropertyChangedEvent();
                    RaisePropertyChangedEvent(nameof(HasTabs));
                }
            }
        }

        public bool HasTabs => Tabs != null && Tabs.Count > 0;

        internal AdaptiveViewModel(string uri) : base()
        {
            if (uri.Contains("&title="))
            {
                const string Value = "&title=";
                Title = uri.Substring(uri.LastIndexOf(Value, StringComparison.Ordinal) + Value.Length);
            }
            Uri = UriHelper.NormalizePageUri(uri);
            Provider = new CoolapkListProvider(
                (p, _, __) => UriHelper.GetUri(UriType.GetIndexPage, Uri, IsIndexPage ? "?" : "&", p),
                GetEntities,
                "entityId");
        }

        internal AdaptiveViewModel(CoolapkListProvider provider)
        {
            Provider = provider;
        }

        public static AdaptiveViewModel GetUserListProvider(string uid, bool isFollowList, string name)
        {
            return string.IsNullOrEmpty(uid)
                ? throw new ArgumentException(nameof(uid))
                : new AdaptiveViewModel(
                new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetUserList,
                            isFollowList ? "followList" : "fansList",
                            uid,
                            p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    (a) => a.Select(o => UserModel.FromJson((isFollowList ? o["fUserInfo"] : o["userInfo"]).AsObject())),
                    "fuid"))
                { Title = $"{name}的{(isFollowList ? "关注" : "粉丝")}" };
        }

        public static AdaptiveViewModel GetReplyListProvider(string id, FeedReplyModel reply = null)
        {
            return string.IsNullOrEmpty(id)
                ? throw new ArgumentException(nameof(id))
                : reply == null
                ? new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetHotReplies,
                                id,
                                p,
                                p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                        (a) => DtoJson.DeserializeList<FeedReplyDto>(a).Select(d => new FeedReplyModel(d)),
                        "uid"))
                { Title = $"热门回复" }
                : new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetReplyReplies,
                                id,
                                p,
                                p > 1 ? $"&lastItem={lastItem}" : string.Empty),
                        (a) => DtoJson.DeserializeList<FeedReplyDto>(a).Select(d => new FeedReplyModel(d, false)),
                        "uid"))
                { Title = $"回复({reply.ReplyNum})" };
        }

        public static AdaptiveViewModel GetHistoryProvider(string title)
        {
            if (string.IsNullOrEmpty(title)) { throw new ArgumentException(nameof(title)); }

            UriType type = UriType.CheckLoginInfo;

            switch (title)
            {
                case "我的常去":
                    type = UriType.GetUserRecentHistory;
                    break;
                case "浏览历史":
                    type = UriType.GetUserHistory;
                    break;
                default: throw new ArgumentException(nameof(title));
            }

            return new AdaptiveViewModel(
                new CoolapkListProvider(
                    (p, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            type,
                            p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    (a) => DtoJson.DeserializeList<HistoryDto>(a).Select(d => new HistoryModel(d)),
                    "uid"))
            { Title = title };
        }

        public static AdaptiveViewModel GetUserFeedsProvider(string uid, string branch)
        {
            return string.IsNullOrEmpty(uid)
                ? throw new ArgumentException(nameof(uid))
                : new AdaptiveViewModel(
                    new CoolapkListProvider(
                        (p, firstItem, lastItem) =>
                            UriHelper.GetUri(
                                UriType.GetUserFeeds,
                                uid,
                                p,
                                string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}",
                                branch),
                        (a) => DtoJson.DeserializeList<FeedDto>(a).Select(d => new FeedModel(d)),
                        "uid"));
        }

        bool IViewModel.IsEqual(IViewModel other) => other is AdaptiveViewModel model && IsEqual(model);

        public bool IsEqual(AdaptiveViewModel other) => !string.IsNullOrWhiteSpace(Uri) ? Uri == other.Uri : Provider == other.Provider;

        private IEnumerable<Entity> GetEntities(JsonArray array)
        {
            foreach (JsonNode node in array)
            {
                JsonObject json = node.AsObject();
                if (json.TryGetPropertyValue("entityTemplate", out JsonNode t) && t?.ToString() == "configCard")
                {
                    ApplyConfigCard(json);
                    yield return null;
                }
                else if (json.TryGetPropertyValue("entityTemplate", out JsonNode tabLink) && tabLink?.ToString() == "iconTabLinkGridCard")
                {
                    BuildTabs(json);
                    yield return null;
                }
                else if (json.TryGetPropertyValue("entityTemplate", out JsonNode tt) && tt?.ToString() == "fabCard") { yield return null; }
                else if (tt?.ToString() == "feedCoolPictureGridCard")
                {
                    foreach (JsonNode item in json["entities"]?.AsArray())
                    {
                        Entity entity = EntityTemplateSelector.GetEntity(item.AsObject(), IsHotFeedPage);
                        if (entity != null)
                        {
                            yield return entity;
                        }
                    }
                }
                else
                {
                    yield return EntityTemplateSelector.GetEntity(json, IsHotFeedPage);
                }
            }
            yield break;
        }

        /// <summary>
        /// 解析 configCard 卡片：仅在有 pageTitle 时更新页面标题，避免无标题的配置卡片（如 withRanking）
        /// 把已显示的标题清空；同时兼容缺失或非法的 extraData。
        /// </summary>
        private void ApplyConfigCard(JsonObject json)
        {
            try
            {
                string extraData = (string)json["extraData"];
                if (string.IsNullOrEmpty(extraData)) { return; }

                JsonObject config = JsonNode.Parse(extraData)?.AsObject();
                string pageTitle = (string)config?["pageTitle"];
                if (!string.IsNullOrEmpty(pageTitle))
                {
                    _ = Dispatcher.EnqueueAsync(() => Title = pageTitle);
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(AdaptiveViewModel)).LogWarning(ex, ex.ExceptionToMessage());
            }
        }

        /// <summary>
        /// 将 TabLink 卡片（iconTabLinkGridCard）的子 tab 展开为独立的 <see cref="ShyHeaderItem"/> 列表，
        /// 供 ShyHeaderListView 渲染顶部可切换的 tab 栏。
        /// 注意：本方法在后台线程被调用，只能解析纯数据；ShyHeaderItem 是 DependencyObject，必须在 UI 线程创建。
        /// </summary>
        private void BuildTabs(JsonObject json)
        {
            if (json["entities"] is not JsonArray entities || entities.Count == 0) { return; }

            List<(string title, string url)> tabs = new List<(string title, string url)>();
            foreach (JsonNode node in entities)
            {
                JsonObject tab = node.AsObject();
                string title = (string)tab["title"];
                string url = (string)tab["url"];
                if (string.IsNullOrEmpty(url)) { continue; }
                tabs.Add((title, url));
            }

            if (tabs.Count > 0)
            {
                _ = Dispatcher.EnqueueAsync(() =>
                {
                    List<ShyHeaderItem> items = new List<ShyHeaderItem>();
                    foreach ((string title, string url) in tabs)
                    {
                        string normalized = UriHelper.NormalizePageUri(url);
                        FeedListItemSource source = new FeedListItemSource(title, new CoolapkListProvider(
                            (p, _, __) => UriHelper.GetUri(UriType.GetIndexPage, normalized, normalized.Contains("?") ? "&" : "?", p),
                            GetEntities,
                            "entityId"));
                        items.Add(new ShyHeaderItem { Header = title, ItemSource = source });
                    }
                    Tabs = items;
                });
            }
        }
    }
}
