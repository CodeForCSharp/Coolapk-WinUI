using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json.Nodes;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Users
{
    public class UserModel : Entity, IUserModel, IHasSubtitle
    {
        private readonly int fansNum;
        int IUserModel.FansNum => fansNum;

        private readonly int followNum;
        int IUserModel.FollowNum => followNum;

        public int UID { get; private set; }
        public int Level { get; private set; }
        public int Status { get; private set; }
        public int RegDate { get; private set; }
        public int Experience { get; private set; }
        public int BlockStatus { get; private set; }

        public string Bio { get; private set; }
        public string FansNum { get; private set; }
        public string UserName { get; private set; }
        public string SubTitle { get; private set; }
        public string LoginTime { get; private set; }
        public string FollowNum { get; private set; }
        public string Description { get; private set; }

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public string Url => $"/u/{UID}";
        public string Title => UserName;

        public ImageModel Pic => UserAvatar;

        public UserModel(JsonObject token) : base(token)
        {
            if (token == null) { return; }

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("uid", out JsonNode uid))
            {
                UID = uid.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("bio", out JsonNode bio))
            {
                Bio = bio.ToString();
            }

            if (token.TryGetPropertyValue("fans", out JsonNode fans))
            {
                fansNum = fans.ToInt32Safe();
                FansNum = $"{fansNum}{loader.GetString("Fan")}";
            }

            if (token.TryGetPropertyValue("level", out JsonNode level))
            {
                Level = level.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("cover", out JsonNode cover))
            {
                Cover = new ImageModel(cover.ToString(), ImageType.OriginImage);
            }

            if (token.TryGetPropertyValue("status", out JsonNode status))
            {
                Status = status.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("regdate", out JsonNode regdate))
            {
                RegDate = regdate.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("username", out JsonNode username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetPropertyValue("logintime", out JsonNode logintime))
            {
                LoginTime = $"{logintime.ToInt64Safe().ConvertUnixTimeStampToReadable()}活跃";
            }

            if (token.TryGetPropertyValue("follow", out JsonNode follow))
            {
                followNum = follow.ToInt32Safe();
                FollowNum = $"{followNum}{loader.GetString("Follow")}";
            }

            if (token.TryGetPropertyValue("experience", out JsonNode experience))
            {
                Experience = experience.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("userAvatar", out JsonNode userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetPropertyValue("block_status", out JsonNode block_status))
            {
                BlockStatus = block_status.ToInt32Safe();
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }
}
