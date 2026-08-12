using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    public class TopicModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string FollowNum { get; private set; }
        public string Description { get; private set; }
        public string CommentNum { get; private set; }
        public string LastUpdate { get; private set; }
        public ImageModel Logo { get; private set; }

        public ImageModel Pic => Logo;

        public TopicModel(TopicDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            if (!string.IsNullOrEmpty(dto.Url))
            {
                Url = dto.Url;
            }

            if (!string.IsNullOrEmpty(dto.Title))
            {
                Title = dto.Title;
            }

            if (!string.IsNullOrEmpty(dto.Follownum))
            {
                FollowNum = dto.Follownum;
            }
            else if (!string.IsNullOrEmpty(dto.FollowNum))
            {
                FollowNum = dto.FollowNum;
            }

            if (!string.IsNullOrEmpty(dto.Logo))
            {
                Logo = new ImageModel(dto.Logo, ImageType.Icon);
            }

            if (!string.IsNullOrEmpty(dto.Newsnum))
            {
                CommentNum = dto.Newsnum;
            }
            else if (!string.IsNullOrEmpty(dto.Commentnum))
            {
                CommentNum = dto.Commentnum;
            }
            else if (!string.IsNullOrEmpty(dto.RatingTotalNum))
            {
                CommentNum = dto.RatingTotalNum;
            }

            if (!string.IsNullOrEmpty(dto.Description))
            {
                Description = dto.Description;
            }
            else if (!string.IsNullOrEmpty(dto.Newtitle))
            {
                Description = dto.Newtitle;
            }
            else if (!string.IsNullOrEmpty(dto.Username))
            {
                Description = "作者" + dto.Username;
            }
            else if (!string.IsNullOrEmpty(dto.RssType))
            {
                Description = dto.RssType;
            }
            else if (dto.HotNum != null)
            {
                Description = DataHelper.GetNumString(dto.HotNum.Value) + "热度";
            }

            if (dto.Lastupdate != null)
            {
                LastUpdate = dto.Lastupdate.Value.ConvertUnixTimeStampToReadable();
            }
        }

        public static TopicModel FromJson(JsonObject json)
            => new TopicModel(JsonSerializer.Deserialize<TopicDto>(json, DtoJson.Options));

        public override string ToString() => $"{Title} - {Description}";
    }
}
