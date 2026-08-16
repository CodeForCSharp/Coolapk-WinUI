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
    internal partial class CollectionDetail : FeedListDetailBase, ICanLike, ICanFollow
    {
        [ObservableProperty]
        public partial string FollowNum { get; set; }

        [ObservableProperty]
        public partial bool Liked { get; set; }

        [ObservableProperty]
        public partial int LikeNum { get; set; }

        public int ID { get; private set; }
        public int ItemNum { get; private set; }

        public string Url { get; private set; }
        public string Title { get; private set; }
        public string UserName { get; private set; }
        public string LastUpdate { get; private set; }
        public string Description { get; private set; }

        public ImageModel Cover { get; private set; }
        public ImageModel UserAvatar { get; private set; }

        internal CollectionDetail(CollectionDetailDto dto) : base(dto)
        {

            ID = dto.Id;
            Followed = dto.UserAction?.Follow == 1;
            Liked = dto.UserAction?.Like == 1;

            ItemNum = dto.ItemNum;
            LikeNum = dto.LikeNum;

            Url = dto.Url;
            Title = dto.Title;
            UserName = dto.Username;
            Description = dto.Description;

            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (dto.FollowNum != null)
            {
                FollowNum = $"{dto.FollowNum}{loader.GetString("SubscribeNum")}";
            }

            if (dto.Lastupdate != null)
            {
                LastUpdate = $"{dto.Lastupdate.Value.ConvertUnixTimeStampToReadable()}活跃";
            }

            if (dto.CoverPic != null)
            {
                Cover = new ImageModel(dto.CoverPic, ImageType.OriginImage);
            }

            if (dto.UserAvatar != null)
            {
                UserAvatar = new ImageModel(dto.UserAvatar, ImageType.BigAvatar);
            }

            OnFollowChanged();
        }

        public static CollectionDetail FromJson(JsonObject json)
            => new CollectionDetail(DtoJson.Deserialize<CollectionDetailDto>(json));

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

        public Task ChangeLike() => FeedActionsService.ChangeCollectionLikeAsync(this);

        public Task ChangeFollow() => FeedActionsService.ChangeCollectionFollowAsync(this);
    }
}
