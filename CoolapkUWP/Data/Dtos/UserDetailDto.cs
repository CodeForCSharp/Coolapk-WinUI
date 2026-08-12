namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户空间详情(user space)。
    /// </summary>
    public class UserDetailDto : EntityDto
    {
        public int Uid { get; set; }
        public int Feed { get; set; }
        public int BeLikeNum { get; set; }
        public int Fans { get; set; }
        public int Level { get; set; }
        public int Follow { get; set; }
        public int IsFans { get; set; }
        public int IsBlackList { get; set; }
        public int IsFollow { get; set; }
        public string Bio { get; set; }
        public string Province { get; set; }
        public string City { get; set; }
        public string Astro { get; set; }
        public string Gender { get; set; }
        public string DisplayUsername { get; set; }
        public long? Logintime { get; set; }
        public int? BlockStatus { get; set; }
        public string VerifyTitle { get; set; }
        public double NextLevelExperience { get; set; }
        public double NextLevelPercentage { get; set; }
        public string Cover { get; set; }
        public string UserAvatar { get; set; }
    }
}
