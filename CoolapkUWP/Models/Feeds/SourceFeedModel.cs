using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text;
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
        public string RatingMessage { get; private set; }
        public bool ShowRatingMessage { get; private set; }

        /// <summary>点评正文按标签拆开，信息流里每段只显示一行。</summary>
        public List<RatingSection> RatingSections { get; private set; } = new List<RatingSection>();

        /// <summary>信息流横排图，最多三张。</summary>
        public List<ImageModel> RatingPreviewPics { get; private set; } = new List<ImageModel>();

        public ImageModel RatingPreviewPic0 => RatingPreviewPics.Count > 0 ? RatingPreviewPics[0] : null;

        public ImageModel RatingPreviewPic1 => RatingPreviewPics.Count > 1 ? RatingPreviewPics[1] : null;

        public ImageModel RatingPreviewPic2 => RatingPreviewPics.Count > 2 ? RatingPreviewPics[2] : null;

        public bool HasRatingPreviewPics => RatingPreviewPics.Count > 0;

        public bool HasOneRatingPic => RatingPreviewPics.Count == 1;

        public bool HasTwoRatingPics => RatingPreviewPics.Count == 2;

        public bool HasManyRatingPics => RatingPreviewPics.Count >= 3;

        /// <summary>超过三张时，第三张右上角显示的剩余张数。</summary>
        public string RatingPicMoreText { get; private set; }

        /// <summary>普通正文。点评已经用分段正文时不再重复显示。</summary>
        public bool ShowPlainMessage => !ShowRatingMessage && !string.IsNullOrEmpty(Message);

        /// <summary>机主评分 / 酷友评分。</summary>
        public string RatingLabel { get; private set; }

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
                    for (int i = 0; i < 5; i++) { TargetStars.Add(i < tstars); }
                }
            }

            if (dto.Dateline != null)
            {
                // 优先使用服务器返回的相对时间（如 "53分钟前"、"1月15日"），与官方客户端一致
                Dateline = !string.IsNullOrEmpty(dto.DatelineText)
                    ? dto.DatelineText
                    : dto.Dateline.Value.ConvertUnixTimeStampToReadable();
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

                RatingPreviewPics = PicArr.Take(3).ToList();
                RatingPicMoreText = PicArr.Count > 3 ? $"{PicArr.Count - 3}+" : string.Empty;
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
            for (int i = 0; i < 5; i++) { OverallStars.Add(i < overallStars); }
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
            }

            RatingContent rating = ReadRatingContent(dto.V4RatingMessage);
            if (rating.Sections.Count == 0)
            {
                rating = ReadRatingContent(dto.Message);
            }
            RatingMessage = rating.Html;
            RatingSections = rating.Sections;
            ShowRatingMessage = IsStructuredRating(RatingMessage);
            RatingLabel = dto.RatingScore >= 1 && dto.RatingScore <= 10 && dto.Star <= 0
                ? "酷友评分"
                : "机主评分";
        }

        private static bool IsStructuredRating(string html)
            => !string.IsNullOrEmpty(html) && html.IndexOf("rating-tag", StringComparison.Ordinal) >= 0;

        /// <summary>
        /// 点评正文是 {"点评":"...","外观质感":"..."} 这种对象。同一次解析同时给出详情 HTML 和信息流分段。
        /// </summary>
        private static RatingContent ReadRatingContent(string raw)
        {
            List<RatingSection> sections = new List<RatingSection>();
            if (string.IsNullOrWhiteSpace(raw)) { return new RatingContent(null, sections); }

            string trimmed = raw.Trim();
            if (trimmed[0] != '[' && trimmed[0] != '{') { return new RatingContent(null, sections); }

            JsonNode root;
            try { root = JsonNode.Parse(trimmed); }
            catch (JsonException) { return new RatingContent(null, sections); }

            StringBuilder builder = new StringBuilder();
            if (root is JsonObject map)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, JsonNode> pair in map)
                {
                    AppendRatingPart(builder, sections, pair.Key, pair.Value?.ToString());
                }
            }
            else if (root is JsonArray items)
            {
                foreach (JsonNode node in items)
                {
                    if (node is not JsonObject item) { continue; }
                    string title = item["title"]?.ToString() ?? item["name"]?.ToString();
                    string body = item["message"]?.ToString() ?? item["content"]?.ToString() ?? item["text"]?.ToString();
                    AppendRatingPart(builder, sections, title, body);
                }
            }

            return new RatingContent(builder.Length > 0 ? builder.ToString() : null, sections);
        }

        private static void AppendRatingPart(StringBuilder builder, List<RatingSection> sections, string title, string body)
        {
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(body)) { return; }

            if (builder.Length > 0) { builder.Append("<br/>"); }
            if (!string.IsNullOrWhiteSpace(title))
            {
                builder.Append("<div class=\"rating-tag\">").Append(System.Net.WebUtility.HtmlEncode(title.Trim())).Append("</div>");
            }
            if (!string.IsNullOrEmpty(body)) { builder.Append(NormalizeRatingBody(body)); }
            sections.Add(new RatingSection(title?.Trim(), StripRatingHtml(body)));
        }

        private static string StripRatingHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) { return string.Empty; }
            string text = System.Net.WebUtility.HtmlDecode(html).Replace("<!--break-->", string.Empty);
            text = System.Text.RegularExpressions.Regex.Replace(text, "<br\\s*/?>", " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            text = System.Text.RegularExpressions.Regex.Replace(text, "<[^>]+>", string.Empty);
            return text.Trim();
        }

        /// <summary>
        /// 点评正文里已经有话题链接和 &lt;!--break--&gt;。整段转义会让这些标签被原样显示出来。
        /// </summary>
        private static string NormalizeRatingBody(string body)
        {
            string text = body.Replace("\r\n", "\n").Replace('\r', '\n').Replace("<!--break-->", string.Empty);
            if (text.IndexOf('<') < 0)
            {
                return System.Net.WebUtility.HtmlEncode(text).Replace("\n", "<br/>");
            }

            StringBuilder builder = new StringBuilder(text.Length);
            bool inTag = false;
            foreach (char c in text)
            {
                if (c == '<') { inTag = true; }
                if (!inTag && c == '\n')
                {
                    builder.Append("<br/>");
                    continue;
                }
                builder.Append(c);
                if (c == '>') { inTag = false; }
            }
            return builder.ToString();
        }

        private readonly struct RatingContent
        {
            public string Html { get; }
            public List<RatingSection> Sections { get; }

            public RatingContent(string html, List<RatingSection> sections)
            {
                Html = html;
                Sections = sections;
            }
        }

        public static SourceFeedModel FromJson(JsonObject json)
            => new SourceFeedModel(DtoJson.Deserialize<FeedDto>(json));

        public override string ToString() => Message;
    }
}
