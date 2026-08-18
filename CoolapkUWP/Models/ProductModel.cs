using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models
{
    /// <summary>
    /// 数码产品列表条目(排行榜等)，展示 logo、标题、价格区间与评分。
    /// </summary>
    public class ProductModel : Entity, IHasDescription, IStarRating
    {
        private static readonly Regex ReleaseDateRegex = new Regex(
            @"^(?<year>\d{4})年(?<month>\d{1,2})月(?:(?<day>\d{1,2})日)?",
            RegexOptions.Compiled);

        public int ID { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public string HotNum { get; private set; }
        public ImageModel Pic { get; private set; }
        public string ReleaseDateShort { get; private set; }
        public string ReleaseDateYear { get; private set; }
        public string Url => $"/product/{ID}";

        /// <summary>评分(0-10，如 "9.3")。</summary>
        public string RatingScore { get; private set; }

        /// <summary>讨论数。</summary>
        public string CommentNum { get; private set; }

        /// <summary>琥珀色星星集合：按评分(0-10 换算 0-5 星)取整填充。</summary>
        public List<bool> TargetStars { get; } = new List<bool>();

        /// <summary>右侧分数榜分数(如续航分/影像分)，未设置时用星级评分。</summary>
        public string RightScore { get; private set; }

        /// <summary>右侧分数榜标签(如 "续航分"/"小时")。</summary>
        public string RightLabel { get; private set; }

        private readonly Dictionary<string, string> _scores = new Dictionary<string, string>();

        public ProductModel(ProductDto dto) : base(dto)
        {

            ID = dto.Id;
            Title = dto.Title;

            Description = BuildDescription(dto);

            _scores["v4_score_item_1_owner_average_score"] = dto.V4ScoreItem1OwnerAverageScore;
            _scores["v4_score_item_2_owner_average_score"] = dto.V4ScoreItem2OwnerAverageScore;
            _scores["v4_score_item_3_owner_average_score"] = dto.V4ScoreItem3OwnerAverageScore;
            _scores["v4_score_item_4_owner_average_score"] = dto.V4ScoreItem4OwnerAverageScore;
            _scores["v4_score_item_5_owner_average_score"] = dto.V4ScoreItem5OwnerAverageScore;
            _scores["v4_score_item_6_owner_average_score"] = dto.V4ScoreItem6OwnerAverageScore;
            _scores["subtab_all_endurance_score"] = dto.SubtabAllEnduranceScore;

            if (!string.IsNullOrEmpty(dto.HotNumTxt))
            {
                HotNum = $"{dto.HotNumTxt}{ResourceLoader.GetForViewIndependentUse("FeedListPage").GetString("HotNum")}";
            }

            if (double.TryParse(dto.RatingAverageScore, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double score) && score > 0)
            {
                RatingScore = dto.RatingAverageScore;
                int stars = System.Math.Max(0, System.Math.Min(5, (int)System.Math.Round(score / 2)));
                for (int i = 0; i < stars; i++) { TargetStars.Add(true); }
            }

            if (!string.IsNullOrEmpty(dto.FeedCommentNumTxt))
            {
                CommentNum = dto.FeedCommentNumTxt;
            }

            if (!string.IsNullOrEmpty(dto.Logo))
            {
                Pic = new ImageModel(dto.Logo, ImageType.Icon);
            }

            ParseReleaseDate(dto.ReleaseTime);
        }

        /// <summary>
        /// 设置右侧分数榜的分数与标签(由榜单 URL 的 rightTopField/rightBottomText 指定)。
        /// </summary>
        public void SetRankingRight(string field, string label)
        {
            if (!string.IsNullOrEmpty(field) && _scores.TryGetValue(field, out string value))
            {
                RightScore = value;
            }
            RightLabel = label;
        }

        private void ParseReleaseDate(string raw)
        {
            if (string.IsNullOrEmpty(raw)) { return; }
            Match match = ReleaseDateRegex.Match(raw);
            if (!match.Success) { return; }
            ReleaseDateYear = match.Groups["year"].Value;
            string month = match.Groups["month"].Value.PadLeft(2, '0');
            Group dayGroup = match.Groups["day"];
            ReleaseDateShort = dayGroup.Success
                ? $"{month}.{dayGroup.Value.PadLeft(2, '0')}"
                : $"{month}月";
        }

        private static string BuildDescription(ProductDto dto)
        {
            string price = string.Empty;
            if (dto.PriceMax > dto.PriceMin)
            {
                price = $"{dto.PriceCurrency}{dto.PriceMin}-{dto.PriceMax}";
            }
            else if (dto.PriceMin > 0)
            {
                price = $"{dto.PriceCurrency}{dto.PriceMin}";
            }
            else if (dto.PriceMax > 0)
            {
                price = $"{dto.PriceCurrency}{dto.PriceMax}";
            }

            string rating = !string.IsNullOrEmpty(dto.RatingAverageScore)
                ? $"{dto.RatingAverageScore}分"
                : string.Empty;

            if (!string.IsNullOrEmpty(price) && !string.IsNullOrEmpty(rating)) { return $"{price} · {rating}"; }
            if (!string.IsNullOrEmpty(price)) { return price; }
            if (!string.IsNullOrEmpty(rating)) { return rating; }
            return dto.Description;
        }

        public static ProductModel FromJson(JsonObject json)
            => new ProductModel(DtoJson.Deserialize<ProductDto>(json));

        public override string ToString() => $"{Title} - {Description}";
    }
}
