using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Pages
{
    internal partial class DyhDetail : FeedListDetailBase, IHasDescription, ICanFollow
    {
        [ObservableProperty]
        public partial string FollowNum { get; set; }

        public int ID { get; private set; }

        public string Uurl { get; private set; }
        public string Title { get; private set; }
        public string UserName { get; private set; }
        public string Description { get; private set; }

        public ImageModel Logo { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        public ImageModel Pic => Logo;

        public string Url => $"/dyh/{ID}";

        internal DyhDetail(DyhDetailDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            ID = dto.Id;
            Followed = dto.UserAction?.Follow == 1;

            if (dto.Uid != null)
            {
                Uurl = $"/u/{dto.Uid}";
            }

            Title = dto.Title;
            UserName = dto.Author;
            Description = dto.Description;

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (dto.Follownum != null)
            {
                FollowNum = $"{dto.Follownum}{loader.GetString("SubscribeNum")}";
            }

            if (dto.Logo != null)
            {
                Logo = new ImageModel(dto.Logo, ImageType.Icon);
            }

            if (dto.UserAvatar != null)
            {
                UserAvatar = new ImageModel(dto.UserAvatar, ImageType.BigAvatar);
            }

            OnFollowChanged();
        }

        public static DyhDetail FromJson(JsonObject json)
            => new DyhDetail(JsonSerializer.Deserialize<DyhDetailDto>(json, DtoJson.Options));

        internal void SetFollowNum(int num)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowNum = $"{num}{loader.GetString("SubscribeNum")}";
        }

        protected override void OnFollowChanged()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unsubscribe") : loader.GetString("Subscribe");
            FollowGlyph = Followed ? "\uE8FB" : "\uE710";
        }

        public Task ChangeFollow() => FeedActionsService.ChangeDyhFollowAsync(this);

        public override string ToString() => $"{Title} - {Description}";
    }

}
