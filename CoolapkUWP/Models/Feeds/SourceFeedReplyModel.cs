using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
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

        public SourceFeedReplyModel(JsonObject token) : base(token)
        {
            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("userInfo", out JsonNode v1))
            {
                JsonObject userInfo = v1.AsObject();
                UserInfo = new UserModel(userInfo);
            }
            else
            {
                UserInfo = new UserModel(null);
            }

            if (token.TryGetPropertyValue("userAction", out JsonNode v2))
            {
                JsonObject userAction = v2.AsObject();
                UserAction = new UserAction(userAction);
            }
            else
            {
                UserAction = new UserAction(null);
            }

            if (token.TryGetPropertyValue("isFeedAuthor", out JsonNode isFeedAuthor))
            {
                IsFeedAuthor = isFeedAuthor.ToInt32Safe() == 1;
            }

            if (token.TryGetPropertyValue("ruid", out JsonNode ruid))
            {
                Rurl = $"/u/{ruid}";
            }

            if (token.TryGetPropertyValue("rusername", out JsonNode rusername))
            {
                Rusername = rusername.ToString();
            }

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");

            if (token.TryGetPropertyValue("message", out JsonNode message))
            {
                Message =
                string.IsNullOrEmpty(Rusername)
                ? $"{GetUserLink(UserInfo.Url, UserInfo.UserName) + GetAuthorString(IsFeedAuthor)}: {message}"
                : $"{GetUserLink(UserInfo.Url, UserInfo.UserName) + GetAuthorString(IsFeedAuthor)}@{GetUserLink(Rurl, Rusername)}: {message}";
            }

            if (token.TryGetPropertyValue("pic", out JsonNode pic) && !string.IsNullOrEmpty(pic.ToString()))
            {
                PicUri = pic.ToString();
                Message += $" <a href=\"{PicUri}\">{loader.GetString("SeePic")}</a>";
            }

            if (token.TryGetPropertyValue("picArr", out JsonNode picArr) && picArr.AsArray().Count > 0 && !string.IsNullOrEmpty(picArr.AsArray()[0].ToString()))
            {
                PicArr = picArr.AsArray().Select(
                    x => !string.IsNullOrEmpty(x.ToString())
                        ? new ImageModel(x.ToString(), ImageType.SmallImage) : null)
                    .Where(x => x != null).ToList();

                foreach (ImageModel item in PicArr)
                {
                    item.ContextArray = PicArr;
                }
            }

            if (token.TryGetPropertyValue("block_status", out JsonNode block_status))
            {
                BlockStatus = block_status.ToInt32Safe();
            }
        }

        private static string GetAuthorString(bool isFeedAuthor) => isFeedAuthor ? TextBlockEx.AuthorBorder : string.Empty;

        private static string GetUserLink(string url, string name) => $"<a href=\"{url}\" type=\"user-detail\">{name}</a>";

        public override string ToString() => Message;
    }
}
