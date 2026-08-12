namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 通知中的用户信息(userInfo/fromUserInfo/likeUserInfo/messageUserInfo)。
    /// </summary>
    public class NotificationUserInfoDto
    {
        public string Status { get; set; }
        public string BlockStatus { get; set; }
        public string Username { get; set; }
        public string UserAvatar { get; set; }
    }
}
