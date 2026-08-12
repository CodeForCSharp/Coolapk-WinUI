namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 通知数字接口(notification/checkCount)响应。
    /// </summary>
    public class NotificationNumbersDto
    {
        public int CloudInstall { get; set; }
        public int Notification { get; set; }
        public int Badge { get; set; }
        public int ContactsFollow { get; set; }
        public int Message { get; set; }
        public int Atme { get; set; }
        public int Atcommentme { get; set; }
        public int Commentme { get; set; }
        public int Feedlike { get; set; }
    }
}
