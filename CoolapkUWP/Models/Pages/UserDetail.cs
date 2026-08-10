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

}
