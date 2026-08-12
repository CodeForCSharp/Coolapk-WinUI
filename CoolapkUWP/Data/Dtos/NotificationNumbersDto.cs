namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 通知数字接口(notification/checkCount)响应。
    /// </summary>
    public class NotificationNumbersDto
    {
        public string CloudInstall { get; set; }
        public string Notification { get; set; }
        public string Badge { get; set; }
        public string ContactsFollow { get; set; }
        public string Message { get; set; }
        public string Atme { get; set; }
        public string Atcommentme { get; set; }
        public string Commentme { get; set; }
        public string Feedlike { get; set; }
    }
}
