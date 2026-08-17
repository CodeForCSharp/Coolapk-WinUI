using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models
{
    /// <summary>
    /// 数码产品列表条目(排行榜等)，展示 logo、标题、价格区间与评分。
    /// </summary>
    public class ProductModel : Entity, IHasDescription
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

        public ProductModel(ProductDto dto) : base(dto)
        {

            ID = dto.Id;
            Title = dto.Title;

            Description = BuildDescription(dto);

            if (!string.IsNullOrEmpty(dto.HotNumTxt))
            {
                HotNum = $"{dto.HotNumTxt}{ResourceLoader.GetForViewIndependentUse("FeedListPage").GetString("HotNum")}";
            }

            if (!string.IsNullOrEmpty(dto.Logo))
            {
                Pic = new ImageModel(dto.Logo, ImageType.Icon);
            }

            ParseReleaseDate(dto.ReleaseTime);
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
