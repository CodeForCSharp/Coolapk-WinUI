using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models.Feeds
{
    internal partial class FeedModel : FeedModelBase
    {
        public bool IsStickTop { get; private set; }
        public bool ShowLikes { get; private set; } = true;
        public bool ShowDateline { get; private set; } = true;

        /// <summary>是否为 feedCover（封面图文）卡片，用于选择专门的模板。</summary>
        public bool IsFeedCover { get; private set; }

        /// <summary>封面大图（message_cover），仅在 feedCover 卡片中存在。</summary>
        public ImageModel MessageCover { get; private set; }

        /// <summary>feedCover 顶部展示的封面：优先 message_cover，缺失时用第一张图。</summary>
        public ImageModel CoverPic => MessageCover ?? (PicArr.Count > 0 ? PicArr[0] : null);

        public FeedModel(FeedDto dto, bool isFirstPageFeed = false) : base(dto)
        {
            ShowLikes = EntityType != "forwardFeed";
            ShowDateline = !isFirstPageFeed;
            IsStickTop = dto.IsStickTop == 1;

            if (!string.IsNullOrEmpty(dto.MessageCover))
            {
                MessageCover = new ImageModel(dto.MessageCover, ImageType.SmallImage);
            }
        }

        public static FeedModel FromJson(JsonObject json, bool isFirstPageFeed = false)
        {
            FeedModel model = new FeedModel(DtoJson.Deserialize<FeedDto>(json), isFirstPageFeed);
            if (json.TryGetPropertyValue("entityTemplate", out JsonNode template) && template?.ToString() == "feedCover")
            {
                model.IsFeedCover = true;
            }
            return model;
        }
    }
}
