using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Images;
using HtmlAgilityPack;
using System.Text.Json.Nodes;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Pages
{
    public abstract class NotificationModel : Entity
    {
        public int ID { get; protected set; }

        public string Url { get; protected set; }
        public string UserUrl { get; protected set; }
        public string UserName { get; protected set; }
        public string Dateline { get; protected set; }
        public string BlockStatus { get; protected set; }

        public ImageModel UserAvatar { get; protected set; }

        protected NotificationModel(JsonObject token) : base(token) { }

        public override string ToString() => $"{UserName} - {Dateline}";
    }

    internal class SimpleNotificationModel : NotificationModel
    {
        public string Note { get; private set; }

        public SimpleNotificationModel(JsonObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("url", out JsonNode url))
            {
                UserUrl = url.ToString();
            }

            if (token.TryGetPropertyValue("dateline", out JsonNode dateline))
            {
                Dateline = dateline.ToInt64Safe().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetPropertyValue("note", out JsonNode _note))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(_note.ToString());
                HtmlNodeCollection nodes = doc.DocumentNode.ChildNodes;
                HtmlNode node = nodes.Last();
                Note = doc.DocumentNode.InnerText;
                Url = node.GetAttributeValue("href", string.Empty);
            }

            if (token.TryGetPropertyValue("fromUserAvatar", out JsonNode fromUserAvatar))
            {
                UserAvatar = new ImageModel(fromUserAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetPropertyValue("fromUserInfo", out JsonNode v1))
            {
                JsonObject fromUserInfo = v1.AsObject();
                BlockStatus = fromUserInfo["status"].ToInt32Safe() == -1 ? loader.GetString("Status-1")
                   : fromUserInfo["block_status"].ToInt32Safe() == -1 ? loader.GetString("BlockStatus-1")
                   : fromUserInfo["block_status"].ToInt32Safe() == 2 ? loader.GetString("BlockStatus2") : null;
            }

            if (token.TryGetPropertyValue("fromusername", out JsonNode fromusername))
            {
                UserName = $"{fromusername} {BlockStatus}";
            }

            if (token.TryGetPropertyValue("block_status", out JsonNode block_status) && block_status.ToString() != "0")
            {
                Dateline += " [已折叠]";
            }

            if (token.TryGetPropertyValue("status", out JsonNode status) && status.ToString() == "-1")
            {
                Dateline += " [仅自己可见]";
            }
        }

        public override string ToString() => Note;
    }

    internal class AtCommentMeNotificationModel : NotificationModel
    {
        public string Message { get; private set; }
        public string FeedMessage { get; private set; }

        public AtCommentMeNotificationModel(JsonObject token) : base(token)
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

            if (token.TryGetPropertyValue("uid", out JsonNode uid))
            {
                UserUrl = $"/u/{uid}";
            }

            if (token.TryGetPropertyValue("dateline", out JsonNode dateline))
            {
                Dateline = dateline.ToInt64Safe().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetPropertyValue("extra_title", out JsonNode extra_title))
            {
                FeedMessage = extra_title.ToString();
            }

            if (token.TryGetPropertyValue("userAvatar", out JsonNode userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            Message = $"{(string.IsNullOrEmpty((string)token["rusername"]) ? string.Empty : $"回复<a href=\"/u/{(string)token["ruid"]}\">{(string)token["rusername"]}</a>: ")}{(string)token["message"]}";

            if (token.TryGetPropertyValue("userInfo", out JsonNode v1))
            {
                JsonObject userInfo = v1.AsObject();
                BlockStatus = userInfo["status"].ToInt32Safe() == -1 ? loader.GetString("Status-1")
                   : userInfo["block_status"].ToInt32Safe() == -1 ? loader.GetString("BlockStatus-1")
                   : userInfo["block_status"].ToInt32Safe() == 2 ? loader.GetString("BlockStatus2") : null;
            }

            if (token.TryGetPropertyValue("username", out JsonNode username))
            {
                UserName = $"{username} {BlockStatus}";
            }

            if (token.TryGetPropertyValue("block_status", out JsonNode block_status) && block_status.ToString() != "0")
            {
                Dateline += " [已折叠]";
            }

            if (token.TryGetPropertyValue("status", out JsonNode status) && status.ToString() == "-1")
            {
                Dateline += " [仅自己可见]";
            }
        }

        public override string ToString() => Message;
    }

    internal class LikeNotificationModel : NotificationModel
    {
        public string Title { get; private set; }

        public SourceFeedModel FeedDetail { get; private set; }

        public LikeNotificationModel(JsonObject token) : base(token)
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

            if (token.TryGetPropertyValue("feedTypeName", out JsonNode feedTypeName))
            {
                Title = $"赞了你的{feedTypeName}";
            }
            else if (token.TryGetPropertyValue("infoHtml", out JsonNode infoHtml))
            {
                Title = $"赞了你的{infoHtml}";
            }

            if (token.TryGetPropertyValue("likeUid", out JsonNode likeUid))
            {
                UserUrl = $"/u/{likeUid}";
            }

            if (token.TryGetPropertyValue("likeTime", out JsonNode likeTime))
            {
                Dateline = likeTime.ToInt64Safe().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetPropertyValue("likeAvatar", out JsonNode likeAvatar))
            {
                UserAvatar = new ImageModel(likeAvatar.ToString(), ImageType.BigAvatar);
            }

            if (token.TryGetPropertyValue("likeUserInfo", out JsonNode v1))
            {
                JsonObject likeUserInfo = v1.AsObject();
                BlockStatus = likeUserInfo["status"].ToInt32Safe() == -1 ? loader.GetString("Status-1")
                   : likeUserInfo["block_status"].ToInt32Safe() == -1 ? loader.GetString("BlockStatus-1")
                   : likeUserInfo["block_status"].ToInt32Safe() == 2 ? loader.GetString("BlockStatus2") : null;
            }

            if (token.TryGetPropertyValue("likeUsername", out JsonNode likeUsername))
            {
                UserName = $"{likeUsername} {BlockStatus}";
            }

            if (token.TryGetPropertyValue("block_status", out JsonNode block_status) && block_status.ToString() != "0")
            {
                Dateline += " [已折叠]";
            }

            if (token.TryGetPropertyValue("status", out JsonNode status) && status.ToString() == "-1")
            {
                Dateline += " [仅自己可见]";
            }

            FeedDetail = new SourceFeedModel(token);
        }

        public override string ToString() => Title;
    }

    internal class MessageNotificationModel : NotificationModel
    {
        public string FeedMessage { get; private set; }

        public MessageNotificationModel(JsonObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("ukey", out JsonNode ukey))
            {
                Url = ukey.ToString();
            }

            if (token.TryGetPropertyValue("uid", out JsonNode uid))
            {
                UserUrl = $"/u/{uid}";
            }

            if (token.TryGetPropertyValue("dateline", out JsonNode dateline))
            {
                Dateline = dateline.ToInt64Safe().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetPropertyValue("message", out JsonNode message))
            {
                FeedMessage = message.ToString();
            }

            if (token.TryGetPropertyValue("messageUserInfo", out JsonNode v1))
            {
                JsonObject messageUserInfo = v1.AsObject();

                if (messageUserInfo.TryGetPropertyValue("userAvatar", out JsonNode userAvatar))
                {
                    UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
                }

                BlockStatus = messageUserInfo["status"].ToInt32Safe() == -1 ? loader.GetString("Status-1")
                   : messageUserInfo["block_status"].ToInt32Safe() == -1 ? loader.GetString("BlockStatus-1")
                   : messageUserInfo["block_status"].ToInt32Safe() == 2 ? loader.GetString("BlockStatus2") : null;

                if (messageUserInfo.TryGetPropertyValue("username", out JsonNode username))
                {
                    UserName = $"{username} {BlockStatus}";
                }

            }

            if (token.TryGetPropertyValue("is_top", out JsonNode is_top) && is_top.ToInt32Safe() == 1)
            {
                Dateline += " " + "[置顶]";
            }
        }
    }
}
