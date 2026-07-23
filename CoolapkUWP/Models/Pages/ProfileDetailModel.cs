using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
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

        public ProfileDetailModel(JsonObject token) : base(token)
        {
            if (token.TryGetPropertyValue("userAvatar", out JsonNode userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetPropertyValue("url", out JsonNode url))
            {
                Url = $"https://www.coolapk.com{url}";
            }

            if (token.TryGetPropertyValue("fans", out JsonNode fans))
            {
                FansNum = fans.ToDoubleSafe();
            }

            if (token.TryGetPropertyValue("feed", out JsonNode feed))
            {
                FeedNum = feed.ToDoubleSafe();
            }

            if (token.TryGetPropertyValue("level", out JsonNode level))
            {
                LevelNum = level.ToDoubleSafe();
            }

            if (token.TryGetPropertyValue("username", out JsonNode username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetPropertyValue("follow", out JsonNode follow))
            {
                FollowNum = follow.ToDoubleSafe();
            }

            if (token.TryGetPropertyValue("level_today_message", out JsonNode level_today_message))
            {
                LevelTodayMessage = level_today_message.ToString();
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
        }
    }
}
