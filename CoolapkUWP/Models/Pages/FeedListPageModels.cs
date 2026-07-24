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
    public abstract class FeedListDetailBase : Entity, INotifyPropertyChanged
    {
        private bool isCopyEnabled;
        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                if (isCopyEnabled != value)
                {
                    isCopyEnabled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        protected FeedListDetailBase(JsonObject token) : base(token)
        {
            EntityFixed = true;
        }
    }

    internal class UserDetail : FeedListDetailBase, IUserModel, ICanFollow
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

        int ICanFollow.ID => UID;

        public int UID { get; private set; }
        public int FeedNum { get; private set; }
        public int LikeNum { get; private set; }
        public int FansNum { get; private set; }
        public int LevelNum { get; private set; }
        public int FollowNum { get; private set; }

        public bool IsFans { get; private set; }
        public bool IsBlackList { get; private set; }

        public string Bio { get; private set; }
        public string City { get; private set; }
        public string Astro { get; private set; }
        public string Gender { get; private set; }
        public string UserName { get; private set; }
        public string LoginTime { get; private set; }
        public string BlockStatus { get; private set; }
        public string VerifyTitle { get; private set; }

        public double NextLevelExperience { get; private set; }
        public double NextLevelPercentage { get; private set; }
        public string NextLevelNowExperience { get; private set; }

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public string Url => $"/u/{UID}";

        internal UserDetail(JsonObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("uid", out JsonNode uid))
            {
                UID = uid.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("feed", out JsonNode feed))
            {
                FeedNum = feed.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("be_like_num", out JsonNode be_like_num))
            {
                LikeNum = be_like_num.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("fans", out JsonNode fans))
            {
                FansNum = fans.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("level", out JsonNode level))
            {
                LevelNum = level.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("follow", out JsonNode follow))
            {
                FollowNum = follow.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("isFans", out JsonNode isFans))
            {
                IsFans = isFans.ToInt32Safe() != 0;
            }

            if (token.TryGetPropertyValue("isBlackList", out JsonNode isBlackList))
            {
                IsBlackList = isBlackList.ToInt32Safe() == 1;
            }

            if (token.TryGetPropertyValue("isFollow", out JsonNode isFollow))
            {
                Followed = isFollow.ToInt32Safe() != 0;
            }

            if (token.TryGetPropertyValue("bio", out JsonNode bio))
            {
                Bio = bio.ToString();
            }

            if (token.TryGetPropertyValue("province", out JsonNode province) && token.TryGetPropertyValue("city", out JsonNode city))
            {
                City = province.ToString() == city.ToString() ? city.ToString() : $"{province} {city}";
            }

            if (token.TryGetPropertyValue("astro", out JsonNode astro))
            {
                Astro = astro.ToString();
            }

            if (token.TryGetPropertyValue("gender", out JsonNode gender))
            {
                Gender = gender.ToInt32Safe() == 1 ? "♂"
                    : gender.ToInt32Safe() == 0 ? "♀"
                    : string.Empty;
            }

            if (token.TryGetPropertyValue("displayUsername", out JsonNode displayUsername))
            {
                UserName = displayUsername.ToString();
            }

            if (token.TryGetPropertyValue("logintime", out JsonNode logintime))
            {
                LoginTime = $"{logintime.ToInt64Safe().ConvertUnixTimeStampToReadable()}活跃";
            }

            if (token.TryGetPropertyValue("block_status", out JsonNode block_status))
            {
                BlockStatus = block_status.ToInt32Safe() == -1 ? loader.GetString("BlockStatus-1")
                    : block_status.ToInt32Safe() == 2 ? loader.GetString("BlockStatus2") : "\0\0";
                BlockStatus = BlockStatus.Substring(1, BlockStatus.Length - 2);
            }

            if (token.TryGetPropertyValue("verify_title", out JsonNode verify_title))
            {
                VerifyTitle = verify_title.ToString();
            }

            if (token.TryGetPropertyValue("next_level_experience", out JsonNode next_level_experience))
            {
                NextLevelExperience = next_level_experience.ToDoubleSafe();
            }

            if (token.TryGetPropertyValue("next_level_percentage", out JsonNode next_level_percentage))
            {
                NextLevelPercentage = next_level_percentage.ToDoubleSafe();
            }

            NextLevelNowExperience = $"{NextLevelPercentage / 100 * NextLevelExperience:F0}/{NextLevelExperience}";

            if (token.TryGetPropertyValue("cover", out JsonNode cover))
            {
                Cover = new ImageModel(cover.ToString(), ImageType.OriginImage);
            }

            if (token.TryGetPropertyValue("userAvatar", out JsonNode userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            if (UID.ToString() != SettingsHelper.Get<string>(SettingsHelper.Uid))
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
                FollowStatus = IsBlackList ? loader.GetString("InBlackList")
                    : Followed ? IsFans ? loader.GetString("UnfollowFan") : loader.GetString("Unfollow")
                    : IsFans ? loader.GetString("FollowFan") : loader.GetString("Follow");
                FollowGlyph = IsBlackList ? "\uE8F8"
                    : Followed ? IsFans ? "\uE8EE" : "\uE8FB"
                    : IsFans ? "\uE97A" : "\uE710";
            }
        }

        public async Task ChangeFollow()
        {
            UriType type = Followed ? UriType.PostUserUnfollow : UriType.PostUserFollow;

            (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, UID), null, true);
            if (!isSucceed) { return; }

            Followed = !Followed;
        }

        public override string ToString() => $"{UserName} - {Bio}";
    }

    internal class TopicDetail : FeedListDetailBase, IHasSubtitle, ICanFollow
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

        public string Url { get; private set; }
        public string Title { get; private set; }
        public string HotNum { get; private set; }
        public string SubTitle { get; private set; }
        public string FollowNum { get; private set; }
        public string CommentNum { get; private set; }
        public string Description { get; private set; }

        public ImageModel Logo { get; private set; }

        public ImageModel Pic => Logo;

        public List<UserModel> FollowUsers { get; private set; } = new List<UserModel>();

        internal TopicDetail(JsonObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("url", out JsonNode url))
            {
                Url = url.ToString();
            }

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("userAction", out JsonNode userAction) && userAction.AsObject().TryGetPropertyValue("follow", out JsonNode follow))
            {
                Followed = follow.ToInt32Safe() == 1;
            }

            if (token.TryGetPropertyValue("hot_num_txt", out JsonNode hot_num_text))
            {
                HotNum = $"{hot_num_text}{loader.GetString("HotNum")}";
            }

            if (token.TryGetPropertyValue("follownum_txt", out JsonNode follownum_text))
            {
                FollowNum = $"{follownum_text}{loader.GetString("Follow")}";
            }

            if (token.TryGetPropertyValue("commentnum_txt", out JsonNode commentnum_text))
            {
                CommentNum = $"{commentnum_text}{loader.GetString("CommentNum")}";
            }

            if (token.TryGetPropertyValue("description", out JsonNode description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }

            if (token.TryGetPropertyValue("intro", out JsonNode intro) && Description != intro.ToString())
            {
                SubTitle = intro.ToString();
            }

            if (token.TryGetPropertyValue("logo", out JsonNode logo))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetPropertyValue("recent_follow_list", out JsonNode recent_follow_list) && (recent_follow_list as JsonArray).Count > 0)
            {
                FollowUsers = recent_follow_list.AsArray().Select(
                    x => x.AsObject().TryGetPropertyValue("userInfo", out JsonNode userInfo)
                        ? new UserModel(userInfo.AsObject()) : null)
                    .Where(x => x != null).ToList();
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
            UriType type = Followed ? UriType.PostTopicUnfollow : UriType.PostTopicFollow;

            (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, Title), null, true);
            if (!isSucceed) { return; }

            Followed = !Followed;
        }

        public override string ToString() => $"{Title} - {Description}";
    }

    internal class DyhDetail : FeedListDetailBase, IHasDescription, ICanFollow
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

        private string followNum;
        public string FollowNum
        {
            get => followNum;
            set
            {
                if (followNum != value)
                {
                    followNum = value;
                    RaisePropertyChangedEvent();
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

        public string Uurl { get; private set; }
        public string Title { get; private set; }
        public string UserName { get; private set; }
        public string Description { get; private set; }

        public ImageModel Logo { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public ImageModel Pic => Logo;

        public string Url => $"/dyh/{ID}";

        internal DyhDetail(JsonObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("userAction", out JsonNode userAction) && userAction.AsObject().TryGetPropertyValue("follow", out JsonNode follow))
            {
                Followed = follow.ToInt32Safe() == 1;
            }

            if (token.TryGetPropertyValue("uid", out JsonNode uid))
            {
                Uurl = $"/u/{uid}";
            }

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("author", out JsonNode author))
            {
                UserName = author.ToString();
            }

            if (token.TryGetPropertyValue("follownum", out JsonNode follownum))
            {
                FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
            }

            if (token.TryGetPropertyValue("description", out JsonNode description))
            {
                Description = description.ToString();
            }

            if (token.TryGetPropertyValue("logo", out JsonNode logo))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetPropertyValue("userAvatar", out JsonNode userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unsubscribe") : loader.GetString("Subscribe");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public async Task ChangeFollow()
        {
            UriType type = Followed ? UriType.PostDyhUnfollow : UriType.PostDyhFollow;

            (bool isSucceed, JsonNode result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, ID), null, true);
            if (!isSucceed) { return; }

            Followed = !Followed;
            if (result.ToInt32Safe() is int follownum && follownum >= 0)
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
                FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }

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

    internal class CollectionDetail : FeedListDetailBase, ICanLike, ICanFollow
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

        private string followNum;
        public string FollowNum
        {
            get => followNum;
            set
            {
                if (followNum != value)
                {
                    followNum = value;
                    RaisePropertyChangedEvent();
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

        private bool liked;
        public bool Liked
        {
            get => liked;
            set
            {
                if (liked != value)
                {
                    liked = value;
                    RaisePropertyChangedEvent();
                }
            }
        }


        private int likeNum;
        public int LikeNum
        {
            get => likeNum;
            set
            {
                if (likeNum != value)
                {
                    likeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int ID { get; private set; }
        public int ItemNum { get; private set; }

        public string Url { get; private set; }
        public string Title { get; private set; }
        public string UserName { get; private set; }
        public string LastUpdate { get; private set; }
        public string Description { get; private set; }

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public CollectionDetail(JsonObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("userAction", out JsonNode userAction))
            {
                if (userAction.AsObject().TryGetPropertyValue("follow", out JsonNode follow))
                {
                    Followed = follow.ToInt32Safe() == 1;
                }

                if (userAction.AsObject().TryGetPropertyValue("like", out JsonNode like))
                {
                    Liked = like.ToInt32Safe() == 1;
                }
            }

            if (token.TryGetPropertyValue("item_num", out JsonNode item_num))
            {
                ItemNum = item_num.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("like_num", out JsonNode like_num))
            {
                LikeNum = like_num.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("url", out JsonNode url))
            {
                Url = url.ToString();
            }

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("username", out JsonNode username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetPropertyValue("follow_num", out JsonNode follownum))
            {
                FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
            }

            if (token.TryGetPropertyValue("lastupdate", out JsonNode lastupdate))
            {
                LastUpdate = $"{lastupdate.ToInt64Safe().ConvertUnixTimeStampToReadable()}活跃";
            }

            if (token.TryGetPropertyValue("description", out JsonNode description))
            {
                Description = description.ToString();
            }

            if (token.TryGetPropertyValue("cover_pic", out JsonNode cover_pic))
            {
                Cover = new ImageModel(cover_pic.ToString(), ImageType.OriginImage);
            }

            if (token.TryGetPropertyValue("userAvatar", out JsonNode userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unsubscribe") : loader.GetString("Subscribe");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public async Task ChangeLike()
        {
            UriType type = Liked ? UriType.PostCollectionUnlike : UriType.PostCollectionLike;

            using (MultipartFormDataContent content = new MultipartFormDataContent())
            using (StringContent id = new StringContent(ID.ToString()))
            {
                content.Add(id, "id");
                (bool isSucceed, JsonNode result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type), content, true);
                if (!isSucceed) { return; }
                Liked = !Liked;
                if (result.ToInt32Safe() is int follownum && follownum >= 0)
                {
                    LikeNum = follownum;
                }
            }
        }

        public async Task ChangeFollow()
        {
            UriType type = Followed ? UriType.PostCollectionUnfollow : UriType.PostCollectionFollow;

            using (MultipartFormDataContent content = new MultipartFormDataContent())
            using (StringContent id = new StringContent(ID.ToString()))
            {
                content.Add(id, "id");
                (bool isSucceed, JsonNode result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type), content, true);
                if (!isSucceed) { return; }
                Followed = !Followed;
                if (result.ToInt32Safe() is int follownum && follownum >= 0)
                {
                    ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
                    FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
                }
            }
        }
    }
}
