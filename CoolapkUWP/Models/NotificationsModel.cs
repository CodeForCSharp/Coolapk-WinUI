using CoolapkUWP.Helpers;
using System.Text.Json.Nodes;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;

namespace CoolapkUWP.Models
{
    public partial class NotificationsModel : INotifyPropertyChanged
    {
        public static NotificationsModel Instance = new NotificationsModel();

        private readonly DispatcherTimer timer;
        private int badgeNum, followNum, messageNum, atMeNum, atCommentMeNum, commentMeNum, feedLikeNum, cloudInstall, notification;

        public int BadgeNum
        {
            get => badgeNum;
            private set
            {
                if (value != badgeNum)
                {
                    badgeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int FollowNum
        {
            get => followNum;
            private set
            {
                if (value != followNum)
                {
                    followNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int MessageNum
        {
            get => messageNum;
            private set
            {
                if (value != messageNum)
                {
                    messageNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int AtMeNum
        {
            get => atMeNum;
            private set
            {
                if (value != atMeNum)
                {
                    atMeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int AtCommentMeNum
        {
            get => atCommentMeNum;
            private set
            {
                if (value != atCommentMeNum)
                {
                    atCommentMeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int CommentMeNum
        {
            get => commentMeNum;
            private set
            {
                if (value != commentMeNum)
                {
                    commentMeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int FeedLikeNum
        {
            get => feedLikeNum;
            private set
            {
                if (value != feedLikeNum)
                {
                    feedLikeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int CloudInstall
        {
            get => cloudInstall;
            private set
            {
                if (value != cloudInstall)
                {
                    cloudInstall = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int Notification
        {
            get => notification;
            private set
            {
                if (value != notification)
                {
                    notification = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

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
