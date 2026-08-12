namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户条目(user)。
    /// </summary>
    public class UserDto : EntityDto
    {
        public int Uid { get; set; }
        public string Bio { get; set; }
        public int? Fans { get; set; }
        public int Level { get; set; }
        public string Cover { get; set; }
        public int Status { get; set; }
        public int Regdate { get; set; }
        public string Username { get; set; }
        public long? Logintime { get; set; }
        public int? Follow { get; set; }
        public int Experience { get; set; }
        public string UserAvatar { get; set; }
        public int BlockStatus { get; set; }
    }
}
