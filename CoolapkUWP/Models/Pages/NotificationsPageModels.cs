using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Images;
using HtmlAgilityPack;
using System.Text.Json;
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

        protected NotificationModel(NotificationDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);
        }

        protected void ApplyCommon(NotificationDto dto)
        {
            ID = dto.Id;

            if (dto.Dateline != null)
            {
                Dateline = dto.Dateline.Value.ConvertUnixTimeStampToReadable();
            }

            if (!string.IsNullOrEmpty(dto.BlockStatus) && dto.BlockStatus != "0")
            {
                Dateline += " [已折叠]";
            }

            if (dto.Status == "-1")
            {
                Dateline += " [仅自己可见]";
            }
        }

        protected static string GetBlockStatus(NotificationUserInfoDto userInfo, ResourceLoader loader)
            => userInfo == null ? null
                : userInfo.Status == -1 ? loader.GetString("Status-1")
                : userInfo.BlockStatus == -1 ? loader.GetString("BlockStatus-1")
                : userInfo.BlockStatus == 2 ? loader.GetString("BlockStatus2")
                : null;

        public override string ToString() => $"{UserName} - {Dateline}";
    }

    internal class SimpleNotificationModel : NotificationModel
    {
        public string Note { get; private set; }

        public SimpleNotificationModel(NotificationDto dto) : base(dto)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            ApplyCommon(dto);

            UserUrl = dto.Url;

            if (dto.Note != null)
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(dto.Note);
                HtmlNodeCollection nodes = doc.DocumentNode.ChildNodes;
                HtmlNode node = nodes.Last();
                Note = doc.DocumentNode.InnerText;
                Url = node.GetAttributeValue("href", string.Empty);
            }

            if (dto.FromUserAvatar != null)
            {
                UserAvatar = new ImageModel(dto.FromUserAvatar, ImageType.BigAvatar);
            }

            BlockStatus = GetBlockStatus(dto.FromUserInfo, loader);

            if (dto.Fromusername != null)
            {
                UserName = $"{dto.Fromusername} {BlockStatus}";
            }
        }

        public override string ToString() => Note;
    }

    internal class AtCommentMeNotificationModel : NotificationModel
    {
        public string Message { get; private set; }
        public string FeedMessage { get; private set; }

        public AtCommentMeNotificationModel(NotificationDto dto) : base(dto)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            ApplyCommon(dto);

            Url = dto.Url;

            if (dto.Uid != null)
            {
                UserUrl = $"/u/{dto.Uid}";
            }

            FeedMessage = dto.ExtraTitle;

            if (dto.UserAvatar != null)
            {
                UserAvatar = new ImageModel(dto.UserAvatar, ImageType.BigAvatar);
            }

            Message = $"{(string.IsNullOrEmpty(dto.Rusername) ? string.Empty : $"回复<a href=\"/u/{dto.Ruid}\">{dto.Rusername}</a>: ")}{dto.Message}";

            BlockStatus = GetBlockStatus(dto.UserInfo, loader);

            if (dto.Username != null)
            {
                UserName = $"{dto.Username} {BlockStatus}";
            }
        }

        public override string ToString() => Message;
    }

    internal class LikeNotificationModel : NotificationModel
    {
        public string Title { get; private set; }

        public SourceFeedModel FeedDetail { get; private set; }

        public LikeNotificationModel(NotificationDto dto, JsonObject raw) : base(dto)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            ApplyCommon(dto);

            Url = dto.Url;

            if (!string.IsNullOrEmpty(dto.FeedTypeName))
            {
                Title = $"赞了你的{dto.FeedTypeName}";
            }
            else if (!string.IsNullOrEmpty(dto.InfoHtml))
            {
                Title = $"赞了你的{dto.InfoHtml}";
            }

            if (dto.LikeUid != null)
            {
                UserUrl = $"/u/{dto.LikeUid}";
            }

            if (dto.LikeTime != null)
            {
                Dateline = dto.LikeTime.Value.ConvertUnixTimeStampToReadable();
            }

            if (dto.LikeAvatar != null)
            {
                UserAvatar = new ImageModel(dto.LikeAvatar, ImageType.BigAvatar);
            }

            BlockStatus = GetBlockStatus(dto.LikeUserInfo, loader);

            if (dto.LikeUsername != null)
            {
                UserName = $"{dto.LikeUsername} {BlockStatus}";
            }

            FeedDetail = SourceFeedModel.FromJson(raw);
        }

        public override string ToString() => Title;
    }

    internal class MessageNotificationModel : NotificationModel
    {
        public string FeedMessage { get; private set; }

        public MessageNotificationModel(NotificationDto dto) : base(dto)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            ApplyCommon(dto);

            Url = dto.Ukey;

            if (dto.Uid != null)
            {
                UserUrl = $"/u/{dto.Uid}";
            }

            FeedMessage = dto.Message;

            if (dto.MessageUserInfo != null)
            {
                if (!string.IsNullOrEmpty(dto.MessageUserInfo.UserAvatar))
                {
                    UserAvatar = new ImageModel(dto.MessageUserInfo.UserAvatar, ImageType.BigAvatar);
                }

                BlockStatus = GetBlockStatus(dto.MessageUserInfo, loader);

                if (!string.IsNullOrEmpty(dto.MessageUserInfo.Username))
                {
                    UserName = $"{dto.MessageUserInfo.Username} {BlockStatus}";
                }
            }

            if (dto.IsTop == 1)
            {
                Dateline += " " + "[置顶]";
            }
        }
    }

    internal static class NotificationModelFactory
    {
        internal static SimpleNotificationModel CreateSimple(JsonObject json)
            => new SimpleNotificationModel(JsonSerializer.Deserialize<NotificationDto>(json, DtoJson.Options));

        internal static AtCommentMeNotificationModel CreateAtCommentMe(JsonObject json)
            => new AtCommentMeNotificationModel(JsonSerializer.Deserialize<NotificationDto>(json, DtoJson.Options));

        internal static LikeNotificationModel CreateLike(JsonObject json)
            => new LikeNotificationModel(JsonSerializer.Deserialize<NotificationDto>(json, DtoJson.Options), json);

        internal static MessageNotificationModel CreateMessage(JsonObject json)
            => new MessageNotificationModel(JsonSerializer.Deserialize<NotificationDto>(json, DtoJson.Options));
    }
}
