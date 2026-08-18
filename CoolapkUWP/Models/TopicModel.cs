using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    public class TopicModel : Entity, IHasDescription, IStarRating
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string FollowNum { get; private set; }
        public string Description { get; private set; }
        public string CommentNum { get; private set; }
        public string LastUpdate { get; private set; }
        public ImageModel Logo { get; private set; }

        public ImageModel Pic => Logo;

        /// <summary>热度(如 "1.2万热度")。</summary>
        public string HotNum { get; private set; }

        /// <summary>评分(0-10，如 "9.5")。</summary>
        public string RatingScore { get; private set; }

        /// <summary>琥珀色星星集合：按评分(0-10 换算 0-5 星)取整填充。</summary>
        public List<bool> TargetStars { get; } = new List<bool>();

        /// <summary>右侧分数榜分数(话题榜单不使用)。</summary>
        public string RightScore { get; private set; }

        /// <summary>右侧分数榜标签(话题榜单不使用)。</summary>
        public string RightLabel { get; private set; }

        public TopicModel(TopicDto dto) : base(dto)
        {

            if (!string.IsNullOrEmpty(dto.Url))
            {
                Url = dto.Url;
            }

            if (!string.IsNullOrEmpty(dto.Title))
            {
                Title = dto.Title;
            }

            if (!string.IsNullOrEmpty(dto.Follownum))
            {
                FollowNum = dto.Follownum;
            }
            else if (!string.IsNullOrEmpty(dto.FollowNum))
            {
                FollowNum = dto.FollowNum;
            }

            if (!string.IsNullOrEmpty(dto.Logo))
            {
                Logo = new ImageModel(dto.Logo, ImageType.Icon);
            }

            if (!string.IsNullOrEmpty(dto.CommentNumTxt))
            {
                CommentNum = dto.CommentNumTxt;
            }
            else if (!string.IsNullOrEmpty(dto.Newsnum))
            {
                CommentNum = dto.Newsnum;
            }
            else if (!string.IsNullOrEmpty(dto.Commentnum))
            {
                CommentNum = dto.Commentnum;
            }
            else if (!string.IsNullOrEmpty(dto.RatingTotalNum))
            {
                CommentNum = dto.RatingTotalNum;
            }

            if (!string.IsNullOrEmpty(dto.HotNumTxt))
            {
                HotNum = $"{dto.HotNumTxt}热度";
            }
            else if (dto.HotNum != null)
            {
                HotNum = DataHelper.GetNumString(dto.HotNum.Value) + "热度";
            }

            if (double.TryParse(dto.StarAverageScore ?? dto.RatingAverageScore, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double score) && score > 0)
            {
                RatingScore = dto.StarAverageScore ?? dto.RatingAverageScore;
                int stars = System.Math.Max(0, System.Math.Min(5, (int)System.Math.Round(score / 2)));
                for (int i = 0; i < stars; i++) { TargetStars.Add(true); }
            }

            if (!string.IsNullOrEmpty(dto.Description))
            {
                Description = dto.Description;
            }
            else if (!string.IsNullOrEmpty(dto.Newtitle))
            {
                Description = dto.Newtitle;
            }
            else if (!string.IsNullOrEmpty(dto.Username))
            {
                Description = "作者" + dto.Username;
            }
            else if (!string.IsNullOrEmpty(dto.RssType))
            {
                Description = dto.RssType;
            }
            else if (dto.HotNum != null)
            {
                Description = DataHelper.GetNumString(dto.HotNum.Value) + "热度";
            }

            if (dto.Lastupdate != null)
            {
                LastUpdate = dto.Lastupdate.Value.ConvertUnixTimeStampToReadable();
            }
        }

        public static TopicModel FromJson(JsonObject json)
            => new TopicModel(DtoJson.Deserialize<TopicDto>(json));

        public override string ToString() => $"{Title} - {Description}";
    }
}
