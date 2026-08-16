using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Feeds
{
    [INotifyPropertyChanged]
    public partial class SourceFeedModel : Entity
    {
        [ObservableProperty]
        public partial bool ShowUser { get; set; } = true;

        [ObservableProperty]
        public partial bool IsCopyEnabled { get; set; }

        public int RatingStar { get; private set; }

        public bool IsVoteFeed { get; private set; }
        public bool IsRatingFeed { get; private set; }
        public bool IsQuestionFeed { get; private set; }

        public string Url { get; private set; }
        public string Message { get; private set; }
        public string Dateline { get; private set; }
        public string ShareUrl { get; private set; }
        public string MessageTitle { get; private set; }
        public string FeedType { get; private set; } = "feed";

        public ImageModel Pic { get; private set; }
        public UserModel UserInfo { get; private set; }
        public UserAction UserAction { get; private set; }

        public List<ImageModel> PicArr { get; private set; } = new List<ImageModel>();

        public SourceFeedModel(FeedDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            if (!string.IsNullOrEmpty(dto.Url))
            {
                Url = dto.Url;
            }
            else if (dto.Id != null)
            {
                Url = $"/feed/{dto.Id.Replace("\"", string.Empty)}";
            }

            UserInfo = dto.UserInfo != null
                ? new UserModel(dto.UserInfo)
                : new UserModel(null);

            UserAction = dto.UserAction != null
                ? new UserAction(dto.UserAction)
                : new UserAction(null);

            ShareUrl = !string.IsNullOrEmpty(dto.ShareUrl)
                ? dto.ShareUrl
                : $"https://www.coolapk.com{(Url != null ? Url.Replace("/question/", "/feed/") : string.Empty)}";

            if (dto.Message != null)
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");
                Message = dto.Message.Replace("<a href=\"\">查看更多</a>", $"<a href=\"{Url}\">{loader.GetString("ReadMore")}</a>");
            }

            MessageTitle = dto.MessageTitle;

            if (dto.FeedType != null)
            {
                FeedType = dto.FeedType;
                switch (FeedType)
                {
                    case "vote":
                        IsVoteFeed = true;
                        Url = Url?.Replace("/feed/", "/vote/");
                        break;
                    case "rating":
                        IsRatingFeed = true;
                        RatingStar = dto.Star;
                        break;
                    case "question":
                        IsQuestionFeed = true;
                        Url = Url?.Replace("/feed/", "/question/");
                        break;
                }
            }

            if (dto.Dateline != null)
            {
                Dateline = dto.Dateline.Value.ConvertUnixTimeStampToReadable();
            }

            if (dto.PicArr != null && dto.PicArr.Count > 0)
            {
                PicArr = dto.PicArr
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => new ImageModel(x, ImageType.SmallImage))
                    .ToList();

                foreach (ImageModel item in PicArr)
                {
                    item.ContextArray = PicArr;
                }
            }

            if (!string.IsNullOrEmpty(dto.Pic))
            {
                Pic = new ImageModel(dto.Pic, ImageType.SmallImage);
            }
        }

        public static SourceFeedModel FromJson(JsonObject json)
            => new SourceFeedModel(DtoJson.Deserialize<FeedDto>(json));

        public override string ToString() => Message;
    }
}
