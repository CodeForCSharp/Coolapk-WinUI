using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
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
                ChangeNumber(JsonSerializer.Deserialize<NotificationNumbersDto>(result.AsObject(), DtoJson.Options));
            }
            catch { Clear(); }
        }

        private void ChangeNumber(NotificationNumbersDto dto)
        {
            if (dto != null)
            {
                CloudInstall = dto.CloudInstall.ToInt32Safe();
                Notification = dto.Notification.ToInt32Safe();
                BadgeNum = dto.Badge.ToInt32Safe();
                FollowNum = dto.ContactsFollow.ToInt32Safe();
                MessageNum = dto.Message.ToInt32Safe();
                AtMeNum = dto.Atme.ToInt32Safe();
                AtCommentMeNum = dto.Atcommentme.ToInt32Safe();
                CommentMeNum = dto.Commentme.ToInt32Safe();
                FeedLikeNum = dto.Feedlike.ToInt32Safe();
            }
        }
    }
}
