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
    internal partial class TopicDetail : FeedListDetailBase, IHasSubtitle, ICanFollow
    {
        [ObservableProperty]
        private bool followed;

        [ObservableProperty]
        private string followGlyph;

        [ObservableProperty]
        private string followStatus;

        partial void OnFollowedChanged(bool value) => OnFollowChanged();

        public int ID { get; private set; }

        public string Url { get; private set; }
        public string Title { get; private set; }
        public string HotNum { get; private set; }
        public string SubTitle { get; private set; }
        public string FollowNum { get; private set; }
        public string CommentNum { get; private set; }
        public string Description { get; private set; }

        public ImageModel Logo { get; private set; }

        public ImageModel Pic => Logo;

        public List<UserModel> FollowUsers { get; private set; } = new List<UserModel>();

        internal TopicDetail(JsonObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("url", out JsonNode url))
            {
                Url = url.ToString();
            }

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("userAction", out JsonNode userAction) && userAction.AsObject().TryGetPropertyValue("follow", out JsonNode follow))
            {
                Followed = follow.ToInt32Safe() == 1;
            }

            if (token.TryGetPropertyValue("hot_num_txt", out JsonNode hot_num_text))
            {
                HotNum = $"{hot_num_text}{loader.GetString("HotNum")}";
            }

            if (token.TryGetPropertyValue("follownum_txt", out JsonNode follownum_text))
            {
                FollowNum = $"{follownum_text}{loader.GetString("Follow")}";
            }

            if (token.TryGetPropertyValue("commentnum_txt", out JsonNode commentnum_text))
            {
                CommentNum = $"{commentnum_text}{loader.GetString("CommentNum")}";
            }

            if (token.TryGetPropertyValue("description", out JsonNode description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }

            if (token.TryGetPropertyValue("intro", out JsonNode intro) && Description != intro.ToString())
            {
                SubTitle = intro.ToString();
            }

            if (token.TryGetPropertyValue("logo", out JsonNode logo))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetPropertyValue("recent_follow_list", out JsonNode recent_follow_list) && (recent_follow_list as JsonArray).Count > 0)
            {
                FollowUsers = recent_follow_list.AsArray().Select(
                    x => x.AsObject().TryGetPropertyValue("userInfo", out JsonNode userInfo)
                        ? new UserModel(userInfo.AsObject()) : null)
                    .Where(x => x != null).ToList();
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unfollow") : loader.GetString("Follow");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public async Task ChangeFollow()
        {
            UriType type = Followed ? UriType.PostTopicUnfollow : UriType.PostTopicFollow;

            (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, Title), null, true);
            if (!isSucceed) { return; }

            Followed = !Followed;
        }

        public override string ToString() => $"{Title} - {Description}";
    }

}
