namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户主页卡片(user profile)。
    /// </summary>
    public class ProfileDetailDto : EntityDto
    {
        public string UserAvatar { get; set; }
        public string Url { get; set; }
        public double Fans { get; set; }
        public double Feed { get; set; }
        public double Level { get; set; }
        public string Username { get; set; }
        public double Follow { get; set; }
        public string LevelTodayMessage { get; set; }
        public double NextLevelExperience { get; set; }
        public double NextLevelPercentage { get; set; }
    }
}
