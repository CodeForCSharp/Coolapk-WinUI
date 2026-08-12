using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CoolapkUWP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Pages
{
    internal partial class TopicDetail : FeedListDetailBase, IHasSubtitle, ICanFollow
    {
        [ObservableProperty]
        public partial bool Followed { get; set; }

        [ObservableProperty]
        public partial string FollowGlyph { get; set; }

        [ObservableProperty]
        public partial string FollowStatus { get; set; }

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

        internal TopicDetail(TopicDetailDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            ID = dto.Id.ToInt32Safe();
            Url = dto.Url;
            Title = dto.Title;
            Followed = dto.UserAction?.Follow.ToInt32Safe() == 1;

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (dto.HotNumTxt != null)
            {
                HotNum = $"{dto.HotNumTxt}{loader.GetString("HotNum")}";
            }

            if (dto.FollownumTxt != null)
            {
                FollowNum = $"{dto.FollownumTxt}{loader.GetString("Follow")}";
            }

            if (dto.CommentnumTxt != null)
            {
                CommentNum = $"{dto.CommentnumTxt}{loader.GetString("CommentNum")}";
            }

            if (!string.IsNullOrEmpty(dto.Description))
            {
                Description = dto.Description;
            }

            if (dto.Intro != null && Description != dto.Intro)
            {
                SubTitle = dto.Intro;
            }

            if (dto.Logo != null)
            {
                Logo = new ImageModel(dto.Logo, ImageType.Icon);
            }

            if (dto.RecentFollowList != null && dto.RecentFollowList.Count > 0)
            {
                FollowUsers = dto.RecentFollowList.Select(
                    x => x.AsObject().TryGetPropertyValue("userInfo", out JsonNode userInfo)
                        ? UserModel.FromJson(userInfo.AsObject()) : null)
                    .Where(x => x != null).ToList();
            }

            OnFollowChanged();
        }

        public static TopicDetail FromJson(JsonObject json)
            => new TopicDetail(JsonSerializer.Deserialize<TopicDetailDto>(json, DtoJson.Options));

        private void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unfollow") : loader.GetString("Follow");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public Task ChangeFollow() => FeedActionsService.ChangeTopicFollowAsync(this);

        public override string ToString() => $"{Title} - {Description}";
    }

}
