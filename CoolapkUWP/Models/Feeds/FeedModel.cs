using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models.Feeds
{
    internal partial class FeedModel : FeedModelBase
    {
        public bool IsStickTop { get; private set; }
        public bool ShowLikes { get; private set; } = true;
        public bool ShowDateline { get; private set; } = true;

        internal enum FeedDisplayMode
        {
            Normal = 0,
            NotShowDyhName = 0x02,
            IsFirstPageFeed = 0x01,
            NotShowMessageTitle = 0x04
        }

        public FeedModel(FeedDto dto, FeedDisplayMode mode = FeedDisplayMode.Normal) : base(dto)
        {
            ShowLikes = !(EntityType == "forwardFeed");
            ShowDateline = mode != FeedDisplayMode.IsFirstPageFeed;
            IsStickTop = dto.IsStickTop.ToInt32Safe() == 1;
        }

        public static FeedModel FromJson(JsonObject json, FeedDisplayMode mode = FeedDisplayMode.Normal)
            => new FeedModel(JsonSerializer.Deserialize<FeedDto>(json, DtoJson.Options), mode);
    }
}
