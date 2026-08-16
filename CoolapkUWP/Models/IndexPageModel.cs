using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class IndexPageModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public string Description { get; private set; }
        public string EntityTemplate { get; private set; }
        public ImageModel Pic { get; private set; }
        public string StarAverageScore { get; private set; }
        public string CommentNum { get; private set; }
        public string RatingTotalNumTxt { get; private set; }
        public double Star1Fill { get; private set; }
        public double Star2Fill { get; private set; }
        public double Star3Fill { get; private set; }
        public double Star4Fill { get; private set; }
        public double Star5Fill { get; private set; }

        public IndexPageModel(IndexPageDto dto) : base(dto)
        {

            EntityTemplate = dto.EntityTemplate;
            Title = dto.Title;
            StarAverageScore = dto.StarAverageScore;
            CommentNum = dto.CommentNum;
            RatingTotalNumTxt = dto.RatingTotalNumTxt;
            SubTitle = dto.SubTitle;
            Description = dto.Description;

            if (double.TryParse(StarAverageScore, out double score))
            {
                score = Math.Max(0, Math.Min(5, score / 2));
                Star1Fill = Math.Max(0, Math.Min(1, score));
                Star2Fill = Math.Max(0, Math.Min(1, score - 1));
                Star3Fill = Math.Max(0, Math.Min(1, score - 2));
                Star4Fill = Math.Max(0, Math.Min(1, score - 3));
                Star5Fill = Math.Max(0, Math.Min(1, score - 4));
            }

            if (!string.IsNullOrEmpty(dto.VideoPlaybackUrl))
            {
                Url = dto.VideoPlaybackUrl;
            }
            else if (!string.IsNullOrEmpty(dto.Url))
            {
                Url = dto.Url;
            }

            if (!string.IsNullOrEmpty(dto.CoverPic))
            {
                Pic = new ImageModel(dto.CoverPic, ImageType.OriginImage);
            }
            else if (!string.IsNullOrEmpty(dto.Pic))
            {
                Pic = new ImageModel(dto.Pic, ImageType.OriginImage);
            }
            else if (!string.IsNullOrEmpty(dto.Logo))
            {
                Pic = new ImageModel(dto.Logo, ImageType.Icon);
            }
            else if (!string.IsNullOrEmpty(dto.PicUrl))
            {
                Pic = new ImageModel(dto.PicUrl, ImageType.Icon);
            }
        }

        public static IndexPageModel FromJson(JsonObject json)
            => new IndexPageModel(DtoJson.Deserialize<IndexPageDto>(json));

        public override string ToString() => $"{Title} - {Description}";
    }

}
