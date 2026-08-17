using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Feeds
{
    [INotifyPropertyChanged]
    public partial class SourceFeedModel : Entity
    {
        [ObservableProperty]
        public partial bool ShowUser { get; set; } = true;

        [ObservableProperty]
        public partial bool IsCopyEnabled { get; set; }

        public int RatingStar { get; private set; }
        public int RatingScore { get; private set; }
        public string FeedTypeName { get; private set; }
        public string OverallRatingDescriptor { get; private set; }
        public string TargetRowTitle { get; private set; }
        public ImageModel TargetRowLogo { get; private set; }
        public string TargetRowUrl { get; private set; }
        public string TargetRowStarAverageScore { get; private set; }
        public int TargetRowStarTotalCount { get; private set; }
        public string TargetRowSubTitle { get; private set; }
        public List<RatingItemInfo> RatingItemInfos { get; private set; } = new List<RatingItemInfo>();
        public string RatingItemsText { get; private set; }

        public List<bool> OverallStars { get; private set; } = new List<bool>();
        public List<bool> TargetStars { get; private set; } = new List<bool>();

        public System.Collections.Generic.IList<bool> GetOverallStars() => OverallStars;
        public System.Collections.Generic.IList<bool> GetTargetStars() => TargetStars;

        public bool IsVoteFeed { get; private set; }
        public bool IsRatingFeed { get; private set; }
        public bool IsQuestionFeed { get; private set; }

        public string Url { get; private set; }
        public string Message { get; private set; }
        public string Dateline { get; private set; }
        public string ShareUrl { get; private set; }
        public string MessageTitle { get; private set; }
        public string FeedType { get; private set; } = "feed";

        public ImageModel Pic { get; private set; }
        public UserModel UserInfo { get; private set; }
        public UserAction UserAction { get; private set; }

        public List<ImageModel> PicArr { get; private set; } = new List<ImageModel>();

        public SourceFeedModel(FeedDto dto) : base(dto)
        {

            if (!string.IsNullOrEmpty(dto.Url))
            {
                Url = dto.Url;
            }
            else if (dto.Id != null)
            {
                Url = $"/feed/{dto.Id.Replace("\"", string.Empty)}";
            }

            UserInfo = dto.UserInfo != null
                ? new UserModel(dto.UserInfo)
                : new UserModel(null);

            UserAction = dto.UserAction != null
                ? new UserAction(dto.UserAction)
                : new UserAction(null);

            ShareUrl = !string.IsNullOrEmpty(dto.ShareUrl)
                ? dto.ShareUrl
                : $"https://www.coolapk.com{(Url != null ? Url.Replace("/question/", "/feed/") : string.Empty)}";

            if (dto.Message != null)
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");
                Message = dto.Message.Replace("<a href=\"\">查看更多</a>", $"<a href=\"{Url}\">{loader.GetString("ReadMore")}</a>");
            }

            MessageTitle = dto.MessageTitle;
            FeedTypeName = dto.FeedTypeName;

            if (dto.FeedType != null)
            {
                FeedType = dto.FeedType;
                switch (FeedType)
                {
                    case "vote":
                        IsVoteFeed = true;
                        Url = Url?.Replace("/feed/", "/vote/");
                        break;
                    case "rating":
                        IsRatingFeed = true;
                        RatingStar = dto.Star;
                        ParseRatingFields(dto);
                        break;
                    case "question":
                        IsQuestionFeed = true;
                        Url = Url?.Replace("/feed/", "/question/");
                        break;
                }
            }

            if (dto.TargetRow != null)
            {
                TargetRowTitle = dto.TargetRow.Title;
                TargetRowUrl = dto.TargetRow.Url;
                TargetRowSubTitle = dto.TargetRow.SubTitle;
                TargetRowStarAverageScore = dto.TargetRow.StarAverageScore;
                TargetRowStarTotalCount = dto.TargetRow.StarTotalCount;
                if (!string.IsNullOrEmpty(dto.TargetRow.Logo))
                {
                    TargetRowLogo = new ImageModel(dto.TargetRow.Logo, ImageType.Icon);
                }
                if (double.TryParse(dto.TargetRow.StarAverageScore, out double ts))
                {
                    int tstars = System.Math.Max(0, System.Math.Min(5, (int)System.Math.Round(ts / 2.0)));
                    for (int i = 0; i < tstars; i++) { TargetStars.Add(true); }
                }
            }

            if (dto.Dateline != null)
            {
                Dateline = dto.Dateline.Value.ConvertUnixTimeStampToReadable();
            }

            if (dto.PicArr != null && dto.PicArr.Count > 0)
            {
                PicArr = dto.PicArr
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => new ImageModel(x, ImageType.SmallImage))
                    .ToList();

                foreach (ImageModel item in PicArr)
                {
                    item.ContextArray = PicArr;
                }
            }

            if (!string.IsNullOrEmpty(dto.Pic))
            {
                Pic = new ImageModel(dto.Pic, ImageType.SmallImage);
            }
        }

        private void ParseRatingFields(FeedDto dto)
        {
            RatingScore = dto.RatingScore > 0 ? dto.RatingScore : dto.Star * 2;
            int overallStars = dto.Star > 0
                ? System.Math.Max(0, System.Math.Min(5, dto.Star))
                : System.Math.Max(0, System.Math.Min(5, (int)System.Math.Round(RatingScore / 2.0)));
            for (int i = 0; i < overallStars; i++) { OverallStars.Add(true); }
            OverallRatingDescriptor = overallStars switch
            {
                1 => "很差",
                2 => "较差",
                3 => "一般",
                4 => "不错",
                5 => "非常好",
                _ => string.Empty,
            };

            if (dto.RatingItemInfo != null && dto.RatingItemInfo.Count > 0)
            {
                foreach (JsonNode n in dto.RatingItemInfo)
                {
                    if (n is not JsonObject item) { continue; }
                    string name = item["name"]?.ToString();
                    int.TryParse(item["v4_score"]?.ToString(), out int v);
                    List<string> desc = new List<string>();
                    if (item["star_desc"] is JsonArray sd)
                    {
                        foreach (JsonNode s in sd) { desc.Add(s?.ToString()); }
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        RatingItemInfos.Add(new RatingItemInfo(name, v, desc));
                    }
                }
                RatingItemsText = string.Join("    ", RatingItemInfos.Select(item => $"{item.Name} {item.Star}\u2606"));
            }
        }

        public static SourceFeedModel FromJson(JsonObject json)
            => new SourceFeedModel(DtoJson.Deserialize<FeedDto>(json));

        public override string ToString() => Message;
    }
}
