using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models.Feeds
{
    internal partial class FeedModel : FeedModelBase
    {
        public bool IsStickTop { get; private set; }
        public bool ShowLikes { get; private set; } = true;
        public bool ShowDateline { get; private set; } = true;

        public FeedModel(FeedDto dto, bool isFirstPageFeed = false) : base(dto)
        {
            ShowLikes = EntityType != "forwardFeed";
            ShowDateline = !isFirstPageFeed;
            IsStickTop = dto.IsStickTop == 1;
        }

        public static FeedModel FromJson(JsonObject json, bool isFirstPageFeed = false)
            => new FeedModel(DtoJson.Deserialize<FeedDto>(json), isFirstPageFeed);
    }
}
