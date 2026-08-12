using System.Text.Json.Nodes;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 产品详情。
    /// </summary>
    public class ProductDetailDto : EntityDto
    {
        public string Id { get; set; }
        public string Star1Count { get; set; }
        public string Star2Count { get; set; }
        public string Star3Count { get; set; }
        public string Star4Count { get; set; }
        public string Star5Count { get; set; }
        public string OwnerStar1Count { get; set; }
        public string OwnerStar2Count { get; set; }
        public string OwnerStar3Count { get; set; }
        public string OwnerStar4Count { get; set; }
        public string OwnerStar5Count { get; set; }
        public string Title { get; set; }
        public string HotNumTxt { get; set; }
        public string StarTotalCount { get; set; }
        public string FollowNumTxt { get; set; }
        public string FeedCommentNumTxt { get; set; }
        public string RatingTotalNum { get; set; }
        public string Description { get; set; }
        public string OwnerStarAverageScore { get; set; }
        public string RatingAverageScore { get; set; }
        public string Logo { get; set; }
        public JsonArray TagArr { get; set; }
        public JsonArray RecentFollowList { get; set; }
        public JsonArray CoverArr { get; set; }
        public UserActionDto UserAction { get; set; }
    }
}
