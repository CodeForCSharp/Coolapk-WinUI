using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CoolapkUWP.Services;
using Microsoft.UI.Xaml;
using System.Text.Json;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Pages
{
    internal partial class ProductDetail : FeedListDetailBase, ICanFollow
    {
        public int ID { get; private set; }

        public string Title { get; private set; }
        public string HotNum { get; private set; }
        public string FollowNum { get; private set; }
        public string CommentNum { get; private set; }
        public string Description { get; private set; }

        public double OwnerScore { get; private set; }
        public double RatingScore { get; private set; }

        public ImageModel Logo { get; private set; }

        public List<string> TagArr { get; private set; } = new List<string>();

        public List<UserModel> FollowUsers { get; private set; } = new List<UserModel>();

        public List<UserModel> RecentFollowers { get; private set; } = new List<UserModel>();

        public List<ProductRatingBar> RatingBars { get; private set; } = new List<ProductRatingBar>();

        public string ScoreText { get; private set; }

        public string StatLine { get; private set; }

        /// <summary>五颗星的图标，点亮的是实心，其余是空心。</summary>
        public List<string> StarGlyphs { get; private set; } = new List<string>();

        public string OwnerReviewText { get; private set; }

        public string DetailReviewText { get; private set; }

        public string SelectedTabName { get; private set; }

        public List<ProductTab> Tabs { get; private set; } = new List<ProductTab>();

        public bool ShowScore => !string.IsNullOrEmpty(ScoreText);

        public bool ShowRatingCard => ShowScore || RatingBars.Count > 0;

        public bool ShowRatingBars => RatingBars.Count > 0;

        public bool ShowOwnerReview => !string.IsNullOrEmpty(OwnerReviewText);

        public bool ShowDetailReview => !string.IsNullOrEmpty(DetailReviewText);

        public bool ShowReviews => ShowOwnerReview || ShowDetailReview;

        public bool ShowFollowers => RecentFollowers.Count > 0;

        public bool ShowStatLine => !string.IsNullOrEmpty(StatLine);

        internal ProductDetail(ProductDetailDto dto) : base(dto)
        {

            ID = dto.Id;

            Followed = dto.UserAction?.Follow == 1;

            Title = dto.Title;

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (dto.HotNumTxt != null)
            {
                HotNum = $"{dto.HotNumTxt}{loader.GetString("HotNum")}";
            }

            if (dto.FollowNumTxt != null)
            {
                FollowNum = $"{dto.FollowNumTxt}{loader.GetString("Follow")}";
            }

            if (dto.FeedCommentNumTxt != null)
            {
                CommentNum = $"{dto.FeedCommentNumTxt}{loader.GetString("CommentNum")}";
            }

            Description = dto.Description;

            OwnerScore = dto.OwnerStarAverageScore;
            RatingScore = dto.RatingAverageScore;

            if (dto.Logo != null)
            {
                Logo = new ImageModel(dto.Logo, ImageType.Icon);
            }

            if (dto.TagArr != null && dto.TagArr.Count > 0)
            {
                TagArr = dto.TagArr.Where(x => x != null).ToList();
            }

            if (dto.RecentFollowList != null && dto.RecentFollowList.Count > 0)
            {
                FollowUsers = dto.RecentFollowList
                    .Where(x => x.UserInfo != null)
                    .Select(x => new UserModel(x.UserInfo))
                    .ToList();
                RecentFollowers = FollowUsers.Take(3).ToList();
            }

            List<string> stats = new List<string>();
            if (!string.IsNullOrEmpty(HotNum)) { stats.Add(HotNum); }
            if (!string.IsNullOrEmpty(CommentNum)) { stats.Add(CommentNum); }
            StatLine = string.Join(" · ", stats);

            double score = OwnerScore > 0 ? OwnerScore : RatingScore;
            if (score > 0)
            {
                ScoreText = score.ToString("0.0");
                int stars = Math.Max(0, Math.Min(5, (int)Math.Round(score / 2)));
                for (int i = 0; i < 5; i++)
                {
                    StarGlyphs.Add(i < stars ? "\uE735" : "\uE734");
                }
            }

            if (!string.IsNullOrEmpty(dto.RatingTotalNum))
            {
                OwnerReviewText = $"{dto.RatingTotalNum}机主点评";
            }

            if (!string.IsNullOrEmpty(dto.StarTotalCount))
            {
                DetailReviewText = $"{dto.StarTotalCount}详细点评";
            }

            OnFollowChanged();
        }

        public static ProductDetail FromJson(JsonObject json)
        {
            ProductDetail detail = new ProductDetail(DtoJson.Deserialize<ProductDetailDto>(json));
            detail.RatingBars = ReadRatingBars(json);
            detail.Tabs = ReadTabs(json, out string selected);
            detail.SelectedTabName = selected;
            return detail;
        }

        private static List<ProductTab> ReadTabs(JsonObject json, out string selected)
        {
            selected = null;
            List<ProductTab> tabs = new List<ProductTab>();
            if (json == null) { return tabs; }

            selected = json["selectedTab"]?.ToString() ?? json["selected_tab"]?.ToString();
            AddTabs(tabs, AsArray(json["tabList"] ?? json["tab_list"]));
            // 续航、到手价、跑分这类机型才有。Pixel 7 这份是空数组，标签就只有 tabList 那 6 个。
            AddTabs(tabs, AsArray(json["subTab"] ?? json["sub_tab"]));
            return tabs;
        }

        private static void AddTabs(List<ProductTab> tabs, JsonArray array)
        {
            if (array == null) { return; }

            foreach (JsonObject item in array.OfType<JsonObject>())
            {
                string title = item["title"]?.ToString();
                string url = item["url"]?.ToString() ?? item["pageUrl"]?.ToString() ?? item["page_url"]?.ToString();
                string pageName = item["page_name"]?.ToString() ?? item["pageName"]?.ToString();
                string subTitle = item["subTitle"]?.ToString() ?? item["sub_title"]?.ToString();
                if (string.IsNullOrEmpty(title) && string.IsNullOrEmpty(pageName)) { continue; }
                tabs.Add(new ProductTab(string.IsNullOrEmpty(title) ? pageName : title, url, pageName, subTitle));
            }
        }

        private static JsonArray AsArray(JsonNode node)
        {
            if (node is JsonArray array) { return array; }
            string text = node?.ToString();
            if (string.IsNullOrEmpty(text) || text[0] != '[') { return null; }
            try { return JsonNode.Parse(text) as JsonArray; }
            catch (JsonException) { return null; }
        }

        private static List<ProductRatingBar> ReadRatingBars(JsonObject json)
        {
            List<ProductRatingBar> bars = new List<ProductRatingBar>();
            if (json == null) { return bars; }

            JsonArray items = AsArray(json["rating_item_info"] ?? json["ratingItemInfo"] ?? json["v4_rating_item"]);
            if (items != null)
            {
                foreach (JsonObject item in items.OfType<JsonObject>())
                {
                    string name = item["name"]?.ToString() ?? item["title"]?.ToString();
                    double score = ReadScore(item, "owner_average_score", "ownerAverageScore", "owner_star_average_score", "ownerStarAverageScore", "average_score", "star_average_score", "v4_score", "score");
                    if (!string.IsNullOrEmpty(name) && score > 0)
                    {
                        bars.Add(new ProductRatingBar(name, score));
                    }
                }
            }

            if (bars.Count > 0) { return bars; }

            for (int i = 1; i <= 8; i++)
            {
                string name = (string)json[$"v4_score_item_{i}_name"]
                    ?? (string)json[$"v4_score_item_{i}_title"]
                    ?? (string)json[$"v4_score_item_{i}_label"];
                double score = ReadScore(json, $"v4_score_item_{i}_owner_average_score", $"v4_score_item_{i}_average_score");
                if (!string.IsNullOrEmpty(name) && score > 0)
                {
                    bars.Add(new ProductRatingBar(name, score));
                }
            }

            return bars;
        }

        private static double ReadScore(JsonObject json, params string[] names)
        {
            foreach (string name in names)
            {
                JsonNode node = json[name];
                if (node == null) { continue; }
                if (double.TryParse(node.ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double score) && score > 0)
                {
                    return score;
                }
            }

            return 0;
        }

        protected override void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unfollow") : loader.GetString("Follow");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public Task ChangeFollow() => FeedActionsService.ChangeProductFollowAsync(this);

        public override string ToString() => $"{Title} - {Description}";
    }

    /// <summary>
    /// 酷安评分里的一条维度：续航、影像、性能这类。
    /// </summary>
    public class ProductRatingBar
    {
        public string Name { get; }

        public double Score { get; }

        public string ScoreText { get; }

        /// <summary>条的长度按 10 分满分折算。用星号列而不是进度条，避免进度条在列表头里把布局循环撑死。</summary>
        public double Percent => Math.Min(100, Score * 10);

        public GridLength ScoreLength => new(Math.Max(Percent, 0.01), GridUnitType.Star);

        public GridLength RestLength => new(Math.Max(100 - Percent, 0.01), GridUnitType.Star);

        public ProductRatingBar(string name, double score)
        {
            Name = name;
            Score = score;
            ScoreText = score.ToString("0.0");
        }
    }

    /// <summary>
    /// 产品详情 tabList 里的一项。没有 url 的标签只显示标题，不去猜列表地址。
    /// </summary>
    public class ProductTab
    {
        public string Title { get; }

        public string Url { get; }

        public string PageName { get; }

        public string SubTitle { get; }

        public ProductTab(string title, string url, string pageName, string subTitle)
        {
            Title = title ?? string.Empty;
            Url = url ?? string.Empty;
            PageName = pageName ?? string.Empty;
            SubTitle = subTitle ?? string.Empty;
        }
    }

}
