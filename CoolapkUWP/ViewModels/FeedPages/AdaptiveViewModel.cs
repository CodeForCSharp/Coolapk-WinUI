using CoolapkUWP.Controls.DataTemplates;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using CommunityToolkit.WinUI;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
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

        internal AdaptiveViewModel(string uri)
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
                    (o) => new Entity[] { UserModel.FromJson((isFollowList ? o["fUserInfo"] : o["userInfo"]).AsObject()) },
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
                        (o) => new Entity[] { FeedReplyModel.FromJson(o) },
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
                        (o) => new Entity[] { FeedReplyModel.FromJson(o, false) },
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
                    (o) => new Entity[] { HistoryModel.FromJson(o) },
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
                        (o) => new Entity[] { FeedModel.FromJson(o) },
                        "uid"));
        }

        bool IViewModel.IsEqual(IViewModel other) => other is AdaptiveViewModel model && IsEqual(model);

        public bool IsEqual(AdaptiveViewModel other) => !string.IsNullOrWhiteSpace(Uri) ? Uri == other.Uri : Provider == other.Provider;

        private IEnumerable<Entity> GetEntities(JsonObject json)
        {
            if (json.TryGetPropertyValue("entityTemplate", out JsonNode t) && t?.ToString() == "configCard")
            {
                JsonObject j = JsonNode.Parse((string)json["extraData"]).AsObject();
                string pageTitle = (string)j["pageTitle"];
                _ = Dispatcher.EnqueueAsync(() => Title = pageTitle);
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
            yield break;
        }
    }
}
