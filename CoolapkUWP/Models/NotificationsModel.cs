using CoolapkUWP.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;
using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace CoolapkUWP.Models
{
    public partial class NotificationsModel : ObservableObject
    {
        public static NotificationsModel Instance = new NotificationsModel();

        private readonly DispatcherTimer timer;

        [ObservableProperty]
        public partial int BadgeNum { get; private set; }

        [ObservableProperty]
        public partial int FollowNum { get; private set; }

        [ObservableProperty]
        public partial int MessageNum { get; private set; }

        [ObservableProperty]
        public partial int AtMeNum { get; private set; }

        [ObservableProperty]
        public partial int AtCommentMeNum { get; private set; }

        [ObservableProperty]
        public partial int CommentMeNum { get; private set; }

        [ObservableProperty]
        public partial int FeedLikeNum { get; private set; }

        [ObservableProperty]
        public partial int CloudInstall { get; private set; }

        [ObservableProperty]
        public partial int Notification { get; private set; }

        public NotificationsModel()
        {
            Instance = Instance ?? this;
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            timer.Tick += async (o, a) =>
            {
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    await Update();
                }
            };
            timer.Start();
        }

        ~NotificationsModel()
        {
            Clear();
            timer.Stop();
        }

        public void Clear() => BadgeNum = FollowNum = MessageNum = AtMeNum = AtCommentMeNum = CommentMeNum = FeedLikeNum = CloudInstall = Notification = 0;

        public async Task Update()
        {
            try
            {
                (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetNotificationNumbers), true);
                if (!isSucceed) { return; }
                ChangeNumber(result.AsObject());
            }
            catch { Clear(); }
        }

        private void ChangeNumber(JsonObject token)
        {
            if (token != null)
            {
                if (token.TryGetPropertyValue("cloudInstall", out JsonNode cloudInstall) && cloudInstall != null)
                {
                    CloudInstall = token["cloudInstall"].ToInt32Safe();
                }
                if (token.TryGetPropertyValue("notification", out JsonNode notification) && notification != null)
                {
                    Notification = token["notification"].ToInt32Safe();
                }
                if (token.TryGetPropertyValue("badge", out JsonNode badge) && badge != null)
                {
                    BadgeNum = token["badge"].ToInt32Safe();
                    UIHelper.SetBadgeNumber(BadgeNum.ToString());
                }
                if (token.TryGetPropertyValue("contacts_follow", out JsonNode contacts_follow) && contacts_follow != null)
                {
                    FollowNum = token["contacts_follow"].ToInt32Safe();
                }
                if (token.TryGetPropertyValue("message", out JsonNode message) && message != null)
                {
                    MessageNum = token["message"].ToInt32Safe();
                }
                if (token.TryGetPropertyValue("atme", out JsonNode atme) && atme != null)
                {
                    AtMeNum = token["atme"].ToInt32Safe();
                }
                if (token.TryGetPropertyValue("atcommentme", out JsonNode atcommentme) && atcommentme != null)
                {
                    AtCommentMeNum = token["atcommentme"].ToInt32Safe();
                }
                if (token.TryGetPropertyValue("commentme", out JsonNode commentme) && commentme != null)
                {
                    CommentMeNum = token["commentme"].ToInt32Safe();
                }
                if (token.TryGetPropertyValue("feedlike", out JsonNode feedlike) && feedlike != null)
                {
                    FeedLikeNum = token["feedlike"].ToInt32Safe();
                }
            }
        }
    }
}
