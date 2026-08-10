using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Pages
{
    internal partial class CollectionDetail : FeedListDetailBase, ICanLike, ICanFollow
    {
        [ObservableProperty]
        private bool followed;

        [ObservableProperty]
        private string followNum;

        [ObservableProperty]
        private string followGlyph;

        [ObservableProperty]
        private string followStatus;

        [ObservableProperty]
        private bool liked;

        [ObservableProperty]
        private int likeNum;

        partial void OnFollowedChanged(bool value) => OnFollowChanged();

        public int ID { get; private set; }
        public int ItemNum { get; private set; }

        public string Url { get; private set; }
        public string Title { get; private set; }
        public string UserName { get; private set; }
        public string LastUpdate { get; private set; }
        public string Description { get; private set; }

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public CollectionDetail(JsonObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("userAction", out JsonNode userAction))
            {
                if (userAction.AsObject().TryGetPropertyValue("follow", out JsonNode follow))
                {
                    Followed = follow.ToInt32Safe() == 1;
                }

                if (userAction.AsObject().TryGetPropertyValue("like", out JsonNode like))
                {
                    Liked = like.ToInt32Safe() == 1;
                }
            }

            if (token.TryGetPropertyValue("item_num", out JsonNode item_num))
            {
                ItemNum = item_num.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("like_num", out JsonNode like_num))
            {
                LikeNum = like_num.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("url", out JsonNode url))
            {
                Url = url.ToString();
            }

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("username", out JsonNode username))
            {
                UserName = username.ToString();
            }

            if (token.TryGetPropertyValue("follow_num", out JsonNode follownum))
            {
                FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
            }

            if (token.TryGetPropertyValue("lastupdate", out JsonNode lastupdate))
            {
                LastUpdate = $"{lastupdate.ToInt64Safe().ConvertUnixTimeStampToReadable()}活跃";
            }

            if (token.TryGetPropertyValue("description", out JsonNode description))
            {
                Description = description.ToString();
            }

            if (token.TryGetPropertyValue("cover_pic", out JsonNode cover_pic))
            {
                Cover = new ImageModel(cover_pic.ToString(), ImageType.OriginImage);
            }

            if (token.TryGetPropertyValue("userAvatar", out JsonNode userAvatar))
            {
                UserAvatar = new ImageModel(userAvatar.ToString(), ImageType.BigAvatar);
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unsubscribe") : loader.GetString("Subscribe");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public async Task ChangeLike()
        {
            UriType type = Liked ? UriType.PostCollectionUnlike : UriType.PostCollectionLike;

            using (MultipartFormDataContent content = new MultipartFormDataContent())
            using (StringContent id = new StringContent(ID.ToString()))
            {
                content.Add(id, "id");
                (bool isSucceed, JsonNode result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type), content, true);
                if (!isSucceed) { return; }
                Liked = !Liked;
                if (result.ToInt32Safe() is int follownum && follownum >= 0)
                {
                    LikeNum = follownum;
                }
            }
        }

        public async Task ChangeFollow()
        {
            UriType type = Followed ? UriType.PostCollectionUnfollow : UriType.PostCollectionFollow;

            using (MultipartFormDataContent content = new MultipartFormDataContent())
            using (StringContent id = new StringContent(ID.ToString()))
            {
                content.Add(id, "id");
                (bool isSucceed, JsonNode result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type), content, true);
                if (!isSucceed) { return; }
                Followed = !Followed;
                if (result.ToInt32Safe() is int follownum && follownum >= 0)
                {
                    ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
                    FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
                }
            }
        }
    }
}
