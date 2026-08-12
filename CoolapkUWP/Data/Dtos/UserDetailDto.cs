namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户空间详情(user space)。
    /// </summary>
    public class UserDetailDto : EntityDto
    {
        public string Uid { get; set; }
        public string Feed { get; set; }
        public string BeLikeNum { get; set; }
        public string Fans { get; set; }
        public string Level { get; set; }
        public string Follow { get; set; }
        public string IsFans { get; set; }
        public string IsBlackList { get; set; }
        public string IsFollow { get; set; }
        public string Bio { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Astro { get; set; }
        public string Gender { get; set; }
        public string DisplayUsername { get; set; }
        public string Logintime { get; set; }
        public string BlockStatus { get; set; }
        public string VerifyTitle { get; set; }
        public string NextLevelExperience { get; set; }
        public string NextLevelPercentage { get; set; }
        public string Cover { get; set; }
        public string UserAvatar { get; set; }
    }
}
