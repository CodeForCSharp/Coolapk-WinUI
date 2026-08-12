namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 通知中的用户信息(userInfo/fromUserInfo/likeUserInfo/messageUserInfo)。
    /// </summary>
    public class NotificationUserInfoDto
    {
        public int Status { get; set; }
        public int BlockStatus { get; set; }
        public string Username { get; set; }
        public string UserAvatar { get; set; }
    }
}
