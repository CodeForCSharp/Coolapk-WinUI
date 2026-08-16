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
                ChangeNumber(DtoJson.Deserialize<NotificationNumbersDto>(result.AsObject()));
            }
            catch { Clear(); }
        }

        private void ChangeNumber(NotificationNumbersDto dto)
        {
            if (dto != null)
            {
                CloudInstall = dto.CloudInstall;
                Notification = dto.Notification;
                BadgeNum = dto.Badge;
                FollowNum = dto.ContactsFollow;
                MessageNum = dto.Message;
                AtMeNum = dto.Atme;
                AtCommentMeNum = dto.Atcommentme;
                CommentMeNum = dto.Commentme;
                FeedLikeNum = dto.Feedlike;
            }
        }
    }
}
