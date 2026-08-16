using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json;
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

        public UserModel(UserDto dto)
        {
            if (dto == null) { return; }

            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            UID = dto.Uid;
            Bio = dto.Bio;

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (dto.Fans != null)
            {
                fansNum = dto.Fans.Value;
                FansNum = $"{fansNum}{loader.GetString("Fan")}";
            }

            Level = dto.Level;

            if (dto.Cover != null)
            {
                Cover = new ImageModel(dto.Cover, ImageType.OriginImage);
            }

            Status = dto.Status;
            RegDate = dto.Regdate;
            UserName = dto.Username;

            if (dto.Logintime != null)
            {
                LoginTime = $"{dto.Logintime.Value.ConvertUnixTimeStampToReadable()}活跃";
            }

            if (dto.Follow != null)
            {
                followNum = dto.Follow.Value;
                FollowNum = $"{followNum}{loader.GetString("Follow")}";
            }

            Experience = dto.Experience;

            if (dto.UserAvatar != null)
            {
                UserAvatar = new ImageModel(dto.UserAvatar, ImageType.BigAvatar);
            }

            BlockStatus = dto.BlockStatus;
        }

        public static UserModel FromJson(JsonObject json)
            => new UserModel(DtoJson.Deserialize<UserDto>(json));

        public override string ToString() => $"{Title} - {Description}";
    }
}
