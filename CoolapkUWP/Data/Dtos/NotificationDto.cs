namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 通知条目,扁平覆盖全部通知模型(简单通知/@评论/点赞/私信)。
    /// </summary>
    public class NotificationDto : EntityDto
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Uid { get; set; }
        public string Ruid { get; set; }
        public string Rusername { get; set; }
        public long? Dateline { get; set; }
        public string BlockStatus { get; set; }
        public string Status { get; set; }
        public NotificationUserInfoDto UserInfo { get; set; }
        public string Username { get; set; }
        public string UserAvatar { get; set; }

        public string Note { get; set; }
        public string FromUserAvatar { get; set; }
        public NotificationUserInfoDto FromUserInfo { get; set; }
        public string Fromusername { get; set; }

        public string ExtraTitle { get; set; }
        public string Message { get; set; }

        public string FeedTypeName { get; set; }
        public string InfoHtml { get; set; }
        public string LikeUid { get; set; }
        public long? LikeTime { get; set; }
        public string LikeAvatar { get; set; }
        public NotificationUserInfoDto LikeUserInfo { get; set; }
        public string LikeUsername { get; set; }

        public string Ukey { get; set; }
        public NotificationUserInfoDto MessageUserInfo { get; set; }
        public int IsTop { get; set; }
    }
}
