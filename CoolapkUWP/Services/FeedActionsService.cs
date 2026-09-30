using CoolapkUWP.Data;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Pages;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CoolapkUWP.Services
{
    /// <summary>
    /// 动态/评论/详情卡片的关注与点赞等网络操作。
    /// </summary>
    internal static class FeedActionsService
    {
        internal static Task ChangeLikeAsync(ICanLike target) => target switch
        {
            FeedModelBase feed => ChangeFeedLikeAsync(feed),
            FeedReplyModel reply => ChangeReplyLikeAsync(reply),
            CollectionDetail collection => ChangeCollectionLikeAsync(collection),
            _ => Task.CompletedTask
        };

        internal static Task ChangeFollowAsync(ICanFollow target) => target switch
        {
            FeedModelBase feed => ChangeFeedFollowAsync(feed),
            TopicDetail topic => ChangeTopicFollowAsync(topic),
            DyhDetail dyh => ChangeDyhFollowAsync(dyh),
            UserDetail user => ChangeUserFollowAsync(user),
            CollectionDetail collection => ChangeCollectionFollowAsync(collection),
            ProductDetail product => ChangeProductFollowAsync(product),
            _ => Task.CompletedTask
        };

        internal static async Task ChangeFeedLikeAsync(FeedModelBase detail)
        {
            UriType type = detail.Liked ? UriType.PostFeedUnlike : UriType.PostFeedLike;
            (bool isSucceed, JsonNode result) = await PostLikeAsync(type, detail.ID);
            if (!isSucceed) { return; }
            detail.Liked = !detail.Liked;
            if (result.AsObject().TryGetPropertyValue("count", out JsonNode count))
            {
                detail.LikeNum = count.ToInt32Safe();
            }
        }

        private static async Task<(bool isSucceed, JsonNode result)> PostLikeAsync(UriType type, object id)
        {
            using (FormUrlEncodedContent content = new FormUrlEncodedContent(new Dictionary<string, string> { ["trace"] = string.Empty }))
            {
                return await RequestHelper.PostDataAsync(UriHelper.GetOldUri(type, id), content, true);
            }
        }

        internal static async Task ChangeFeedFollowAsync(FeedModelBase detail)
        {
            UriType type = detail.Followed ? UriType.PostUserUnfollow : UriType.PostUserFollow;

            (bool isSucceed, _) = await RequestHelper.GetDataAsync(UriHelper.GetUri(type, detail.UID), true);
            if (!isSucceed) { return; }

            detail.Followed = !detail.Followed;
        }

        internal static async Task ChangeReplyLikeAsync(FeedReplyModel reply)
        {
            UriType type = reply.Liked ? UriType.PostReplyUnlike : UriType.PostReplyLike;
            (bool isSucceed, JsonNode result) = await PostLikeAsync(type, reply.ID);
            if (!isSucceed) { return; }
            reply.Liked = !reply.Liked;
            if (result.ToInt32Safe() is int likenum && likenum >= 0)
            {
                reply.LikeNum = likenum;
            }
        }

        internal static async Task ChangeTopicFollowAsync(TopicDetail detail)
        {
            if (await ChangeTopicFollowByTitleAsync(detail.Title, detail.Followed))
            {
                detail.Followed = !detail.Followed;
            }
        }

        internal static async Task<bool> ChangeTopicFollowByTitleAsync(string title, bool currentlyFollowed)
        {
            if (string.IsNullOrEmpty(title)) { return false; }

            UriType type = currentlyFollowed ? UriType.PostTopicUnfollow : UriType.PostTopicFollow;
            (bool isSucceed, _) = await RequestHelper.GetDataAsync(UriHelper.GetUri(type, title), true);
            return isSucceed;
        }

        internal static async Task ChangeDyhFollowAsync(DyhDetail detail)
        {
            UriType type = detail.Followed ? UriType.PostDyhUnfollow : UriType.PostDyhFollow;

            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(type, detail.ID), true);
            if (!isSucceed) { return; }

            detail.Followed = !detail.Followed;
            if (result.ToInt32Safe() is int follownum && follownum >= 0)
            {
                detail.SetFollowNum(follownum);
            }
        }

        internal static async Task ChangeUserFollowAsync(UserDetail detail)
        {
            UriType type = detail.Followed ? UriType.PostUserUnfollow : UriType.PostUserFollow;

            (bool isSucceed, _) = await RequestHelper.GetDataAsync(UriHelper.GetUri(type, detail.UID), true);
            if (!isSucceed) { return; }

            detail.Followed = !detail.Followed;
        }

        internal static async Task ChangeCollectionLikeAsync(CollectionDetail detail)
        {
            UriType type = detail.Liked ? UriType.PostCollectionUnlike : UriType.PostCollectionLike;

            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(type, detail.ID), true);
            if (!isSucceed) { return; }
            detail.Liked = !detail.Liked;
            if (result.ToInt32Safe() is int likenum && likenum >= 0)
            {
                detail.LikeNum = likenum;
            }
        }

        internal static async Task ChangeCollectionFollowAsync(CollectionDetail detail)
        {
            UriType type = detail.Followed ? UriType.PostCollectionUnfollow : UriType.PostCollectionFollow;

            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(type, detail.ID), true);
            if (!isSucceed) { return; }
            detail.Followed = !detail.Followed;
            if (result.ToInt32Safe() is int follownum && follownum >= 0)
            {
                detail.SetFollowNum(follownum);
            }
        }

        internal static async Task ChangeProductFollowAsync(ProductDetail detail)
        {
            if (await ChangeProductFollowByIdAsync(detail.ID, detail.Followed))
            {
                detail.Followed = !detail.Followed;
            }
        }

        internal static async Task<bool> ChangeProductFollowByIdAsync(int id, bool currentlyFollowed)
        {
            if (id <= 0) { return false; }

            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (StringContent idContent = new StringContent(id.ToString()))
                using (StringContent status = new StringContent(currentlyFollowed ? "0" : "1"))
                {
                    content.Add(idContent, "id");
                    content.Add(status, "status");
                    (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(UriType.OperateProductFollow), content, true);
                    return isSucceed;
                }
            }
        }
    }
}
