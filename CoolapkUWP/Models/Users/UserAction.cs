using CoolapkUWP.Helpers;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Users
{
    [INotifyPropertyChanged]
    public partial class UserAction : Entity
    {
        [ObservableProperty]
        public partial bool Like { get; set; }

        [ObservableProperty]
        public partial bool Favorite { get; set; }

        [ObservableProperty]
        public partial bool Follow { get; set; }

        [ObservableProperty]
        public partial bool Collect { get; set; }

        [ObservableProperty]
        public partial bool FollowAuthor { get; set; }

        [ObservableProperty]
        public partial bool AuthorFollowYou { get; set; }

        [ObservableProperty]
        public partial string FollowGlyph { get; set; }

        [ObservableProperty]
        public partial string FollowStatus { get; set; }

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
