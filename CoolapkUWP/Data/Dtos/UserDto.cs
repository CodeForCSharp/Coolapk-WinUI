namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户条目(user)。
    /// </summary>
    public class UserDto : EntityDto
    {
        public string Uid { get; set; }
        public string Bio { get; set; }
        public string Fans { get; set; }
        public string Level { get; set; }
        public string Cover { get; set; }
        public string Status { get; set; }
        public string Regdate { get; set; }
        public string Username { get; set; }
        public string Logintime { get; set; }
        public string Follow { get; set; }
        public string Experience { get; set; }
        public string UserAvatar { get; set; }
        public string BlockStatus { get; set; }
    }
}
