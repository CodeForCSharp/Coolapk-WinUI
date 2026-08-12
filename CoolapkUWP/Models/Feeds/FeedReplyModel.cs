using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoolapkUWP.Models.Feeds
{
    [INotifyPropertyChanged]
    public partial class FeedReplyModel : SourceFeedReplyModel, ICanLike, ICanReply, ICanCopy
    {
        [ObservableProperty]
        public partial int LikeNum { get; set; }

        [ObservableProperty]
        public partial int ReplyNum { get; set; }

        [ObservableProperty]
        public partial bool IsCopyEnabled { get; set; }

        public bool Liked
        {
            get => UserAction.Like;
            set
            {
                if (UserAction.Like != value)
                {
                    UserAction.Like = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ReplyRowsMore { get; private set; }
        public int ReplyRowsCount { get; private set; }

        public string Dateline { get; private set; }

        public ImageModel Pic { get; private set; }

        public List<SourceFeedReplyModel> ReplyRows { get; private set; } = new List<SourceFeedReplyModel>();

        public FeedReplyModel(FeedReplyDto dto, bool ShowReplyRow = true) : base(dto)
        {
            if (dto.Dateline != null)
            {
                Dateline = dto.Dateline.Value.ConvertUnixTimeStampToReadable();
            }

            if (dto.Message != null)
            {
                Message = dto.Message;
            }

            LikeNum = dto.Likenum;
            ReplyNum = dto.Replynum;
            ReplyRowsMore = dto.ReplyRowsMore;
            ReplyRowsCount = dto.ReplyRowsCount;

            if (dto.ReplyRows != null)
            {
                ReplyRows = dto.ReplyRows.Select(item => new SourceFeedReplyModel(item)).ToList();
            }

            if (!string.IsNullOrEmpty(PicUri))
            {
                Pic = new ImageModel(PicUri, ImageType.SmallImage);
            }
        }

        public static FeedReplyModel FromJson(JsonObject json, bool showReplyRow = true)
            => new FeedReplyModel(JsonSerializer.Deserialize<FeedReplyDto>(json, DtoJson.Options), showReplyRow);
    }
}
