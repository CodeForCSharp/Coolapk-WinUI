using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CoolapkUWP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
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
        public int Star1Count { get; private set; }
        public int Star2Count { get; private set; }
        public int Star3Count { get; private set; }
        public int Star4Count { get; private set; }
        public int Star5Count { get; private set; }
        public int OwnerStar1Count { get; private set; }
        public int OwnerStar2Count { get; private set; }
        public int OwnerStar3Count { get; private set; }
        public int OwnerStar4Count { get; private set; }
        public int OwnerStar5Count { get; private set; }

        public string Title { get; private set; }
        public string HotNum { get; private set; }
        public string StarCount { get; private set; }
        public string FollowNum { get; private set; }
        public string CommentNum { get; private set; }
        public string RatingCount { get; private set; }
        public string Description { get; private set; }

        public double OwnerScore { get; private set; }
        public double RatingScore { get; private set; }
        public double Star1Percent { get; private set; }
        public double Star2Percent { get; private set; }
        public double Star3Percent { get; private set; }
        public double Star4Percent { get; private set; }
        public double Star5Percent { get; private set; }
        public double OwnerStar1Percent { get; private set; }
        public double OwnerStar2Percent { get; private set; }
        public double OwnerStar3Percent { get; private set; }
        public double OwnerStar4Percent { get; private set; }
        public double OwnerStar5Percent { get; private set; }

        public ImageModel Logo { get; private set; }

        public List<string> TagArr { get; private set; } = new List<string>();

        public List<UserModel> FollowUsers { get; private set; } = new List<UserModel>();

        public List<ImageModel> CoverArr { get; private set; } = new List<ImageModel>();

        internal ProductDetail(ProductDetailDto dto) : base(dto)
        {

            ID = dto.Id;

            Star1Count = dto.Star1Count;
            Star2Count = dto.Star2Count;
            Star3Count = dto.Star3Count;
            Star4Count = dto.Star4Count;
            Star5Count = dto.Star5Count;

            OwnerStar1Count = dto.OwnerStar1Count;
            OwnerStar2Count = dto.OwnerStar2Count;
            OwnerStar3Count = dto.OwnerStar3Count;
            OwnerStar4Count = dto.OwnerStar4Count;
            OwnerStar5Count = dto.OwnerStar5Count;

            double MaxStarCount = Math.Max(Math.Max(Math.Max(Star1Count, Star2Count), Math.Max(Star3Count, Star4Count)), Star5Count);
            double MaxOwnerStarCount = Math.Max(Math.Max(Math.Max(OwnerStar1Count, OwnerStar2Count), Math.Max(OwnerStar3Count, OwnerStar4Count)), OwnerStar5Count);
            MaxStarCount = Math.Max(MaxStarCount, double.Epsilon);
            MaxOwnerStarCount = Math.Max(MaxOwnerStarCount, double.Epsilon);

            Star1Percent = Star1Count * 100 / MaxStarCount;
            Star2Percent = Star2Count * 100 / MaxStarCount;
            Star3Percent = Star3Count * 100 / MaxStarCount;
            Star4Percent = Star4Count * 100 / MaxStarCount;
            Star5Percent = Star5Count * 100 / MaxStarCount;

            OwnerStar1Percent = OwnerStar1Count * 100 / MaxOwnerStarCount;
            OwnerStar2Percent = OwnerStar2Count * 100 / MaxOwnerStarCount;
            OwnerStar3Percent = OwnerStar3Count * 100 / MaxOwnerStarCount;
            OwnerStar4Percent = OwnerStar4Count * 100 / MaxOwnerStarCount;
            OwnerStar5Percent = OwnerStar5Count * 100 / MaxOwnerStarCount;

            Followed = dto.UserAction?.Follow == 1;

            Title = dto.Title;

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (dto.HotNumTxt != null)
            {
                HotNum = $"{dto.HotNumTxt}{loader.GetString("HotNum")}";
            }

            if (dto.StarTotalCount != null)
            {
                StarCount = $"{dto.StarTotalCount}位酷友打分";
            }

            if (dto.FollowNumTxt != null)
            {
                FollowNum = $"{dto.FollowNumTxt}{loader.GetString("Follow")}";
            }

            if (dto.FeedCommentNumTxt != null)
            {
                CommentNum = $"{dto.FeedCommentNumTxt}{loader.GetString("CommentNum")}";
            }

            if (dto.RatingTotalNum != null)
            {
                RatingCount = $"{dto.RatingTotalNum}位机主打分";
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
            }

            if (dto.CoverArr != null && dto.CoverArr.Count > 0)
            {
                CoverArr = dto.CoverArr
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => new ImageModel(x, ImageType.SmallImage))
                    .ToList();

                foreach (ImageModel item in CoverArr)
                {
                    item.ContextArray = CoverArr;
                }
            }

            OnFollowChanged();
        }

        public static ProductDetail FromJson(JsonObject json)
            => new ProductDetail(DtoJson.Deserialize<ProductDetailDto>(json));

        protected override void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unfollow") : loader.GetString("Follow");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public Task ChangeFollow() => FeedActionsService.ChangeProductFollowAsync(this);

        public override string ToString() => $"{Title} - {Description}";
    }

}
