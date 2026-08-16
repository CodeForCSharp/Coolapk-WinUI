using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models.Pages
{
    public class ProfileDetailModel : Entity
    {
        public ImageModel UserAvatar { get; private set; }
        public string Url { get; private set; }
        public double FansNum { get; private set; }
        public double FeedNum { get; private set; }
        public double LevelNum { get; private set; }
        public string UserName { get; private set; }
        public double FollowNum { get; private set; }
        public string LevelTodayMessage { get; private set; }
        public double NextLevelExperience { get; private set; }
        public double NextLevelPercentage { get; private set; }
        public string NextLevelNowExperience { get; private set; }

        public ProfileDetailModel(ProfileDetailDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            if (dto.UserAvatar != null)
            {
                UserAvatar = new ImageModel(dto.UserAvatar, ImageType.BigAvatar);
            }

            if (dto.Url != null)
            {
                Url = $"https://www.coolapk.com{dto.Url}";
            }

            FansNum = dto.Fans;
            FeedNum = dto.Feed;
            LevelNum = dto.Level;
            UserName = dto.Username;
            FollowNum = dto.Follow;
            LevelTodayMessage = dto.LevelTodayMessage;

            NextLevelExperience = dto.NextLevelExperience;
            NextLevelPercentage = dto.NextLevelPercentage;
            NextLevelNowExperience = $"{NextLevelPercentage / 100 * NextLevelExperience:F0}/{NextLevelExperience}";
        }

        public static ProfileDetailModel FromJson(JsonObject json)
            => new ProfileDetailModel(DtoJson.Deserialize<ProfileDetailDto>(json));
    }
}
