using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
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

        public UserAction(UserActionDto dto)
        {
            if (dto == null) { return; }

            Like = dto.Like != 0;
            Favorite = dto.Favorite != 0;
            Follow = dto.Follow != 0;
            Collect = dto.Collect != 0;
            FollowAuthor = dto.FollowAuthor != 0;
            AuthorFollowYou = dto.AuthorFollowYou != 0;

            OnFollowChanged();
        }

        public static UserAction FromJson(JsonObject json)
            => new UserAction(DtoJson.Deserialize<UserActionDto>(json));

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
