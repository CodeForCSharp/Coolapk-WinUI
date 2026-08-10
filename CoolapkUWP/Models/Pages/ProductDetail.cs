using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Pages
{
    internal class ProductDetail : FeedListDetailBase, ICanFollow
    {
        private bool followed;
        public bool Followed
        {
            get => followed;
            set
            {
                if (followed != value)
                {
                    followed = value;
                    RaisePropertyChangedEvent();
                    OnFollowChanged();
                }
            }
        }

        private string followGlyph;
        public string FollowGlyph
        {
            get => followGlyph;
            set
            {
                if (followGlyph != value)
                {
                    followGlyph = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string followStatus;
        public string FollowStatus
        {
            get => followStatus;
            set
            {
                if (followStatus != value)
                {
                    followStatus = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

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

        internal ProductDetail(JsonObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            double MaxStarCount = 0, MaxOwnerStarCount = 0;

            if (token.TryGetPropertyValue("star_1_count", out JsonNode star_1_count))
            {
                Star1Count = star_1_count.ToInt32Safe();
                MaxStarCount = Math.Max(Star1Count, MaxStarCount);
            }

            if (token.TryGetPropertyValue("star_2_count", out JsonNode star_2_count))
            {
                Star2Count = star_2_count.ToInt32Safe();
                MaxStarCount = Math.Max(Star2Count, MaxStarCount);
            }

            if (token.TryGetPropertyValue("star_3_count", out JsonNode star_3_count))
            {
                Star3Count = star_3_count.ToInt32Safe();
                MaxStarCount = Math.Max(Star3Count, MaxStarCount);
            }

            if (token.TryGetPropertyValue("star_4_count", out JsonNode star_4_count))
            {
                Star4Count = star_4_count.ToInt32Safe();
                MaxStarCount = Math.Max(Star4Count, MaxStarCount);
            }

            if (token.TryGetPropertyValue("star_5_count", out JsonNode star_5_count))
            {
                Star5Count = star_5_count.ToInt32Safe();
                MaxStarCount = Math.Max(Star5Count, MaxStarCount);
            }

            if (token.TryGetPropertyValue("owner_star_1_count", out JsonNode owner_star_1_count))
            {
                OwnerStar1Count = owner_star_1_count.ToInt32Safe();
                MaxOwnerStarCount = Math.Max(OwnerStar1Count, MaxOwnerStarCount);
            }

            if (token.TryGetPropertyValue("owner_star_2_count", out JsonNode owner_star_2_count))
            {
                OwnerStar2Count = owner_star_2_count.ToInt32Safe();
                MaxOwnerStarCount = Math.Max(OwnerStar2Count, MaxOwnerStarCount);
            }

            if (token.TryGetPropertyValue("owner_star_3_count", out JsonNode owner_star_3_count))
            {
                OwnerStar3Count = owner_star_3_count.ToInt32Safe();
                MaxOwnerStarCount = Math.Max(OwnerStar3Count, MaxOwnerStarCount);
            }

            if (token.TryGetPropertyValue("owner_star_4_count", out JsonNode owner_star_4_count))
            {
                OwnerStar4Count = owner_star_4_count.ToInt32Safe();
                MaxOwnerStarCount = Math.Max(OwnerStar4Count, MaxOwnerStarCount);
            }

            if (token.TryGetPropertyValue("owner_star_5_count", out JsonNode owner_star_5_count))
            {
                OwnerStar5Count = owner_star_5_count.ToInt32Safe();
                MaxOwnerStarCount = Math.Max(OwnerStar5Count, MaxOwnerStarCount);
            }

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

            if (token.TryGetPropertyValue("userAction", out JsonNode userAction) && userAction.AsObject().TryGetPropertyValue("follow", out JsonNode follow))
            {
                Followed = follow.ToInt32Safe() == 1;
            }

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("hot_num_txt", out JsonNode hot_num_text))
            {
                HotNum = $"{hot_num_text}{loader.GetString("HotNum")}";
            }

            if (token.TryGetPropertyValue("star_total_count", out JsonNode star_total_count))
            {
                StarCount = $"{star_total_count}位酷友打分";
            }

            if (token.TryGetPropertyValue("follow_num_txt", out JsonNode follownum_text))
            {
                FollowNum = $"{follownum_text}{loader.GetString("Follow")}";
            }

            if (token.TryGetPropertyValue("feed_comment_num_txt", out JsonNode commentnum_text))
            {
                CommentNum = $"{commentnum_text}{loader.GetString("CommentNum")}";
            }

            if (token.TryGetPropertyValue("rating_total_num", out JsonNode rating_total_num))
            {
                RatingCount = $"{rating_total_num}位机主打分";
            }

            if (token.TryGetPropertyValue("description", out JsonNode description))
            {
                Description = description.ToString();
            }

            if (token.TryGetPropertyValue("owner_star_average_score", out JsonNode owner_star_average_score))
            {
                OwnerScore = owner_star_average_score.ToDoubleSafe();
            }

            if (token.TryGetPropertyValue("rating_average_score", out JsonNode rating_average_score))
            {
                RatingScore = rating_average_score.ToDoubleSafe();
            }

            if (token.TryGetPropertyValue("logo", out JsonNode logo))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetPropertyValue("tagArr", out JsonNode tagArr) && (tagArr as JsonArray).Count > 0)
            {
                TagArr = tagArr.AsArray().Select(x => x.ToString()).ToList();
            }

            if (token.TryGetPropertyValue("recent_follow_list", out JsonNode recent_follow_list) && (recent_follow_list as JsonArray).Count > 0)
            {
                FollowUsers = recent_follow_list.AsArray().Select(
                    x => x.AsObject().TryGetPropertyValue("userInfo", out JsonNode userInfo)
                        ? new UserModel(userInfo.AsObject()) : null)
                    .Where(x => x != null).ToList();
            }

            if (token.TryGetPropertyValue("coverArr", out JsonNode coverArr) && (coverArr as JsonArray).Count > 0)
            {
                CoverArr = coverArr.AsArray().Select(
                    x => !string.IsNullOrEmpty(x.ToString())
                        ? new ImageModel(x.ToString(), ImageType.SmallImage) : null)
                    .Where(x => x != null).ToList();

                foreach (ImageModel item in CoverArr)
                {
                    item.ContextArray = CoverArr;
                }
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unfollow") : loader.GetString("Follow");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public async Task ChangeFollow()
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (StringContent id = new StringContent(ID.ToString()))
                using (StringContent status = new StringContent(Followed ? "0" : "1"))
                {
                    content.Add(id, "id");
                    content.Add(status, "status");
                    (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(UriType.OperateProductFollow), content, true);
                    if (!isSucceed) { return; }
                    Followed = !Followed;
                }
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }

}
