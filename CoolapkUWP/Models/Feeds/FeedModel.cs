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

        public FeedModel(JsonObject token, FeedDisplayMode mode = FeedDisplayMode.Normal) : base(token)
        {
            ShowLikes = !(EntityType == "forwardFeed");
            ShowDateline = mode != FeedDisplayMode.IsFirstPageFeed;
            IsStickTop = token.TryGetPropertyValue("isStickTop", out JsonNode j) && int.Parse(j.ToString()) == 1;
        }
    }
}
