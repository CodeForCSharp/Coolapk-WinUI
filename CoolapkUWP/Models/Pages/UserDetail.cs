using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Pages
{
    internal partial class UserDetail : FeedListDetailBase, IUserModel, ICanFollow
    {
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

        internal UserDetail(UserDetailDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            UID = dto.Uid;
            FeedNum = dto.Feed;
            LikeNum = dto.BeLikeNum;
            FansNum = dto.Fans;
            LevelNum = dto.Level;
            FollowNum = dto.Follow;

            IsFans = dto.IsFans != 0;
            IsBlackList = dto.IsBlackList == 1;
            Followed = dto.IsFollow != 0;

            Bio = dto.Bio;

            if (dto.Province != null && dto.City != null)
            {
                City = dto.Province == dto.City ? dto.City : $"{dto.Province} {dto.City}";
            }

            Astro = dto.Astro;

            int gender = dto.Gender.ToInt32Safe();
            Gender = gender == 1 ? "♂"
                    : gender == 0 ? "♀"
                    : string.Empty;

            UserName = dto.DisplayUsername;

            if (dto.Logintime != null)
            {
                LoginTime = $"{dto.Logintime.Value.ConvertUnixTimeStampToReadable()}活跃";
            }

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (dto.BlockStatus != null)
            {
                int blockStatus = dto.BlockStatus.Value;
                BlockStatus = blockStatus == -1 ? loader.GetString("BlockStatus-1")
                    : blockStatus == 2 ? loader.GetString("BlockStatus2") : "\0\0";
                BlockStatus = BlockStatus.Substring(1, BlockStatus.Length - 2);
            }

            VerifyTitle = dto.VerifyTitle;

            NextLevelExperience = dto.NextLevelExperience;
            NextLevelPercentage = dto.NextLevelPercentage;
            NextLevelNowExperience = $"{NextLevelPercentage / 100 * NextLevelExperience:F0}/{NextLevelExperience}";

            if (dto.Cover != null)
            {
                Cover = new ImageModel(dto.Cover, ImageType.OriginImage);
            }

            if (dto.UserAvatar != null)
            {
                UserAvatar = new ImageModel(dto.UserAvatar, ImageType.BigAvatar);
            }

            OnFollowChanged();
        }

        public static UserDetail FromJson(JsonObject json)
            => new UserDetail(JsonSerializer.Deserialize<UserDetailDto>(json, DtoJson.Options));

        protected override void OnFollowChanged()
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

        public Task ChangeFollow() => FeedActionsService.ChangeUserFollowAsync(this);

        public override string ToString() => $"{UserName} - {Bio}";
    }

}
