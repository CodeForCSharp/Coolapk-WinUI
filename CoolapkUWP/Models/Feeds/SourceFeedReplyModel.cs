using CoolapkUWP.Controls;
using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Feeds
{
    public class SourceFeedReplyModel : Entity
    {
        public int ID { get; private set; }
        public int BlockStatus { get; private set; }

        public bool IsFeedAuthor { get; private set; }

        public string Rurl { get; private set; }
        public string PicUri { get; private set; }
        public string Message { get; protected set; }
        public string Rusername { get; private set; }

        public UserModel UserInfo { get; private set; }
        public UserAction UserAction { get; private set; }

        public List<ImageModel> PicArr { get; private set; } = new List<ImageModel>();

        public SourceFeedReplyModel(FeedReplyDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            ID = dto.Id.ToInt32Safe();

            UserInfo = dto.UserInfo != null
                ? new UserModel(dto.UserInfo)
                : new UserModel(null);

            UserAction = dto.UserAction != null
                ? new UserAction(dto.UserAction)
                : new UserAction(null);

            IsFeedAuthor = dto.IsFeedAuthor.ToInt32Safe() == 1;

            if (dto.Ruid != null)
            {
                Rurl = $"/u/{dto.Ruid}";
            }

            Rusername = dto.Rusername;

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");

            if (dto.Message != null)
            {
                Message =
                string.IsNullOrEmpty(Rusername)
                ? $"{GetUserLink(UserInfo.Url, UserInfo.UserName) + GetAuthorString(IsFeedAuthor)}: {dto.Message}"
                : $"{GetUserLink(UserInfo.Url, UserInfo.UserName) + GetAuthorString(IsFeedAuthor)}@{GetUserLink(Rurl, Rusername)}: {dto.Message}";
            }

            if (!string.IsNullOrEmpty(dto.Pic))
            {
                PicUri = dto.Pic;
                Message += $" <a href=\"{PicUri}\">{loader.GetString("SeePic")}</a>";
            }

            if (dto.PicArr != null && dto.PicArr.Count > 0 && !string.IsNullOrEmpty(dto.PicArr[0]))
            {
                PicArr = dto.PicArr
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => new ImageModel(x, ImageType.SmallImage))
                    .ToList();

                foreach (ImageModel item in PicArr)
                {
                    item.ContextArray = PicArr;
                }
            }

            BlockStatus = dto.BlockStatus.ToInt32Safe();
        }

        public static SourceFeedReplyModel FromJson(JsonObject json)
            => new SourceFeedReplyModel(JsonSerializer.Deserialize<FeedReplyDto>(json, DtoJson.Options));

        private static string GetAuthorString(bool isFeedAuthor) => isFeedAuthor ? TextBlockEx.AuthorBorder : string.Empty;

        private static string GetUserLink(string url, string name) => $"<a href=\"{url}\" type=\"user-detail\">{name}</a>";

        public override string ToString() => Message;
    }
}
