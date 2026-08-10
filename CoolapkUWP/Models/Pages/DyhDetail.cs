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
    internal partial class DyhDetail : FeedListDetailBase, IHasDescription, ICanFollow
    {
        [ObservableProperty]
        private bool followed;

        [ObservableProperty]
        private string followNum;

        [ObservableProperty]
        private string followGlyph;

        [ObservableProperty]
        private string followStatus;

        partial void OnFollowedChanged(bool value) => OnFollowChanged();

        public int ID { get; private set; }

        public string Uurl { get; private set; }
        public string Title { get; private set; }
        public string UserName { get; private set; }
        public string Description { get; private set; }

        public ImageModel Logo { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public ImageModel Pic => Logo;

        public string Url => $"/dyh/{ID}";

        internal DyhDetail(JsonObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("userAction", out JsonNode userAction) && userAction.AsObject().TryGetPropertyValue("follow", out JsonNode follow))
            {
                Followed = follow.ToInt32Safe() == 1;
            }

            if (token.TryGetPropertyValue("uid", out JsonNode uid))
            {
                Uurl = $"/u/{uid}";
            }

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("author", out JsonNode author))
            {
                UserName = author.ToString();
            }

            if (token.TryGetPropertyValue("follownum", out JsonNode follownum))
            {
                FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
            }

            if (token.TryGetPropertyValue("description", out JsonNode description))
            {
                Description = description.ToString();
            }

            if (token.TryGetPropertyValue("logo", out JsonNode logo))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
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

        public async Task ChangeFollow()
        {
            UriType type = Followed ? UriType.PostDyhUnfollow : UriType.PostDyhFollow;

            (bool isSucceed, JsonNode result) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, ID), null, true);
            if (!isSucceed) { return; }

            Followed = !Followed;
            if (result.ToInt32Safe() is int follownum && follownum >= 0)
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
                FollowNum = $"{follownum}{loader.GetString("SubscribeNum")}";
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }

}
