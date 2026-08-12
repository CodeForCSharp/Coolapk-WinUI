using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Pages;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CoolapkUWP.Services
{
    /// <summary>
    /// 列表页详情卡片的关注/点赞等网络操作。
    /// </summary>
    internal static class FeedActionsService
    {
        internal static async Task ChangeTopicFollowAsync(TopicDetail detail)
        {
            UriType type = detail.Followed ? UriType.PostTopicUnfollow : UriType.PostTopicFollow;

            (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, detail.Title), null, true);
            if (!isSucceed) { return; }

            detail.Followed = !detail.Followed;
        }

        internal static async Task ChangeDyhFollowAsync(DyhDetail detail)
        {
            UriType type = detail.Followed ? UriType.PostDyhUnfollow : UriType.PostDyhFollow;

            (bool isSucceed, JsonNode result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, detail.ID), null, true);
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

            (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, detail.UID), null, true);
            if (!isSucceed) { return; }

            detail.Followed = !detail.Followed;
        }

        internal static async Task ChangeCollectionLikeAsync(CollectionDetail detail)
        {
            UriType type = detail.Liked ? UriType.PostCollectionUnlike : UriType.PostCollectionLike;

            using (MultipartFormDataContent content = new MultipartFormDataContent())
            using (StringContent id = new StringContent(detail.ID.ToString()))
            {
                content.Add(id, "id");
                (bool isSucceed, JsonNode result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type), content, true);
                if (!isSucceed) { return; }
                detail.Liked = !detail.Liked;
                if (result.ToInt32Safe() is int likenum && likenum >= 0)
                {
                    detail.LikeNum = likenum;
                }
            }
        }

        internal static async Task ChangeCollectionFollowAsync(CollectionDetail detail)
        {
            UriType type = detail.Followed ? UriType.PostCollectionUnfollow : UriType.PostCollectionFollow;

            using (MultipartFormDataContent content = new MultipartFormDataContent())
            using (StringContent id = new StringContent(detail.ID.ToString()))
            {
                content.Add(id, "id");
                (bool isSucceed, JsonNode result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type), content, true);
                if (!isSucceed) { return; }
                detail.Followed = !detail.Followed;
                if (result.ToInt32Safe() is int follownum && follownum >= 0)
                {
                    detail.SetFollowNum(follownum);
                }
            }
        }

        internal static async Task ChangeProductFollowAsync(ProductDetail detail)
        {
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                using (StringContent id = new StringContent(detail.ID.ToString()))
                using (StringContent status = new StringContent(detail.Followed ? "0" : "1"))
                {
                    content.Add(id, "id");
                    content.Add(status, "status");
                    (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(UriType.OperateProductFollow), content, true);
                    if (!isSucceed) { return; }
                    detail.Followed = !detail.Followed;
                }
            }
        }
    }
}
