using System.Collections.Generic;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 产品详情。
    /// </summary>
    public class ProductDetailDto : EntityDto
    {
        public int Id { get; set; }
        public int Star1Count { get; set; }
        public int Star2Count { get; set; }
        public int Star3Count { get; set; }
        public int Star4Count { get; set; }
        public int Star5Count { get; set; }
        public int OwnerStar1Count { get; set; }
        public int OwnerStar2Count { get; set; }
        public int OwnerStar3Count { get; set; }
        public int OwnerStar4Count { get; set; }
        public int OwnerStar5Count { get; set; }
        public string Title { get; set; }
        public string HotNumTxt { get; set; }
        public string StarTotalCount { get; set; }
        public string FollowNumTxt { get; set; }
        public string FeedCommentNumTxt { get; set; }
        public string RatingTotalNum { get; set; }
        public string Description { get; set; }
        public double OwnerStarAverageScore { get; set; }
        public double RatingAverageScore { get; set; }
        public string Logo { get; set; }
        public List<string> TagArr { get; set; }
        public List<RecentFollowDto> RecentFollowList { get; set; }
        public List<string> CoverArr { get; set; }
        public UserActionDto UserAction { get; set; }
    }
}
