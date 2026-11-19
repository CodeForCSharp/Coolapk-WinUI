using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json.Nodes;
using System.Collections.Generic;

namespace CoolapkUWP.ViewModels.FeedPages
{
    [WinRT.GeneratedBindableCustomProperty]
    public partial class ReplyItemSource : EntityItemSource
    {
        private bool authorOnly;
        private string replyListType = "lastupdate_desc";

        public ReplyItemSource(string id) : base(id)
        {
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                    UriHelper.GetUri(
                        UriType.GetFeedReplies,
                        id,
                        replyListType,
                        p,
                        UriHelper.GetPagingArgs(p, firstItem, lastItem),
                        authorOnly ? 1 : 0),
                GetReplies,
                "id");
        }

        public void SelectFilter(string filter)
        {
            bool nextAuthorOnly = filter == "author";
            string nextListType = filter switch
            {
                "latest" => "dateline_desc",
                "hot" => "popular",
                _ => "lastupdate_desc"
            };
            if (authorOnly == nextAuthorOnly && replyListType == nextListType) { return; }

            authorOnly = nextAuthorOnly;
            replyListType = nextListType;
            _ = Refresh(true);
        }

        /// <summary>整页一次反序列化。逐条 FromJson 会把每个节点再序列化一遍。</summary>
        private static IEnumerable<Entity> GetReplies(JsonArray array)
        {
            foreach (FeedReplyDto dto in DtoJson.DeserializeList<FeedReplyDto>(array))
            {
                if (dto == null) { continue; }
                yield return dto.EntityType == "feed_reply"
                    ? new FeedReplyModel(dto)
                    : new NullEntity();
            }
        }
    }
}
