namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户主页卡片(user profile)。
    /// </summary>
    public class ProfileDetailDto : EntityDto
    {
        public string UserAvatar { get; set; }
        public string Url { get; set; }
        public string Fans { get; set; }
        public string Feed { get; set; }
        public string Level { get; set; }
        public string Username { get; set; }
        public string Follow { get; set; }
        public string LevelTodayMessage { get; set; }
        public string NextLevelExperience { get; set; }
        public string NextLevelPercentage { get; set; }
    }
}
