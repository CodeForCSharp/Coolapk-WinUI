using CoolapkUWP.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Users
{
    public partial class UserAction : Entity, INotifyPropertyChanged
    {
        [ObservableProperty]
        private bool like;

        [ObservableProperty]
        private bool favorite;

        [ObservableProperty]
        private bool follow;

        [ObservableProperty]
        private bool collect;

        [ObservableProperty]
        private bool followAuthor;

        [ObservableProperty]
        private bool authorFollowYou;

        [ObservableProperty]
        private string followGlyph;

        [ObservableProperty]
        private string followStatus;

        partial void OnFollowAuthorChanged(bool value) => OnFollowChanged();

        partial void OnAuthorFollowYouChanged(bool value) => OnFollowChanged();

        public UserAction(JsonObject token) : base(token)
        {
            if (token == null) { return; }

            if (token.TryGetPropertyValue("like", out JsonNode like))
            {
                Like = like.ToInt32Safe() != 0;
            }

            if (token.TryGetPropertyValue("favorite", out JsonNode favorite))
            {
                Favorite = favorite.ToInt32Safe() != 0;
            }

            if (token.TryGetPropertyValue("follow", out JsonNode follow))
            {
                Follow = follow.ToInt32Safe() != 0;
            }

            if (token.TryGetPropertyValue("collect", out JsonNode collect))
            {
                Collect = collect.ToInt32Safe() != 0;
            }

            if (token.TryGetPropertyValue("followAuthor", out JsonNode followAuthor))
            {
                FollowAuthor = followAuthor.ToInt32Safe() != 0;
            }

            if (token.TryGetPropertyValue("authorFollowYou", out JsonNode authorFollowYou))
            {
                AuthorFollowYou = authorFollowYou.ToInt32Safe() != 0;
            }

            OnFollowChanged();
        }

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowGlyph = FollowAuthor ? AuthorFollowYou ? "\uE8EE" : "\uE8FB"
                        : AuthorFollowYou ? "\uE97A" : "\uE710";
            FollowStatus = FollowAuthor ? AuthorFollowYou ? loader.GetString("UnfollowFan") : loader.GetString("Unfollow")
                        : AuthorFollowYou ? loader.GetString("FollowFan") : loader.GetString("Follow");
        }
    }
}
