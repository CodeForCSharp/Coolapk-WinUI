using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;

namespace CoolapkUWP.Models.Feeds
{
    [INotifyPropertyChanged]
    public partial class FeedReplyModel : SourceFeedReplyModel, ICanLike, ICanReply, ICanCopy
    {
        [ObservableProperty]
        public partial int LikeNum { get; set; }

        [ObservableProperty]
        public partial int ReplyNum { get; set; }

        [ObservableProperty]
        public partial bool IsCopyEnabled { get; set; }

        public bool Liked
        {
            get => UserAction.Like;
            set
            {
                if (UserAction.Like != value)
                {
                    UserAction.Like = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ReplyRowsMore { get; private set; }
        public int ReplyRowsCount { get; private set; }

        public string Dateline { get; private set; }

        public ImageModel Pic { get; private set; }

        public List<SourceFeedReplyModel> ReplyRows { get; private set; } = new List<SourceFeedReplyModel>();

        public FeedReplyModel(JsonObject token, bool ShowReplyRow = true) : base(token)
        {
            if (token.TryGetPropertyValue("dateline", out JsonNode dateline))
            {
                Dateline = dateline.ToInt64Safe().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetPropertyValue("message", out JsonNode message))
            {
                Message = message.ToString();
            }

            if (token.TryGetPropertyValue("likenum", out JsonNode likenum))
            {
                LikeNum = likenum.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("replynum", out JsonNode replynum))
            {
                ReplyNum = replynum.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("replyRowsMore", out JsonNode replyRowsMore))
            {
                ReplyRowsMore = replyRowsMore.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("replyRowsCount", out JsonNode replyRowsCount))
            {
                ReplyRowsCount = replyRowsCount.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("replyRows", out JsonNode replyRows))
            {
                ReplyRows = replyRows.AsArray().Select(item => new SourceFeedReplyModel(item.AsObject())).ToList();
            }

            if (!string.IsNullOrEmpty(PicUri))
            {
                Pic = new ImageModel(PicUri, ImageType.SmallImage);
            }
        }

        public async Task ChangeLike()
        {
            UriType type = Liked ? UriType.PostFeedUnlike : UriType.PostFeedLike;
            (bool isSucceed, JsonNode result) = await RequestHelper.PostDataAsync(UriHelper.GetOldUri(type, "Reply", ID), null, true);
            if (!isSucceed) { return; }
            Liked = !Liked;
            if (result.ToInt32Safe() is int likenum && likenum >= 0)
            {
                LikeNum = likenum;
            }
        }
    }
}
