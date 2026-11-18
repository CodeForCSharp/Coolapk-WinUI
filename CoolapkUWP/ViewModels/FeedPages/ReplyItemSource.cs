using CoolapkUWP.Controls;
using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.ViewModels.FeedPages
{
    [WinRT.GeneratedBindableCustomProperty]
    public partial class ReplyItemSource : EntityItemSource, INotifyPropertyChanged, ICanComboBoxChangeSelectedIndex, ICanToggleChangeSelectedIndex
    {
        public List<string> ItemSource { get; private set; }
        private readonly ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedShellPage");

        private bool toggleIsOn;
        public bool ToggleIsOn
        {
            get => toggleIsOn;
            set
            {
                if (toggleIsOn != value)
                {
                    toggleIsOn = value;
                    _ = Refresh(true);
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ToggleIsOn)));
                }
            }
        }

        private string replyListType = "lastupdate_desc";
        public string ReplyListType
        {
            get => replyListType;
            set
            {
                if (replyListType != value)
                {
                    replyListType = value;
                    _ = Refresh(true);
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ReplyListType)));
                }
            }
        }

        public int comboBoxSelectedIndex;
        public int ComboBoxSelectedIndex
        {
            get => comboBoxSelectedIndex;
            set
            {
                if (comboBoxSelectedIndex != value)
                {
                    comboBoxSelectedIndex = value;
                    SetComboBoxSelectedIndex(value);
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(ComboBoxSelectedIndex)));
                }
            }
        }

        public ReplyItemSource(string id) : base(id)
        {
            ItemSource = new List<string>()
            {
                loader.GetString("lastupdate_desc"),
                loader.GetString("dateline_desc"),
                loader.GetString("popular")
            };
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                    UriHelper.GetUri(
                        UriType.GetFeedReplies,
                        id,
                        ReplyListType,
                        p,
                        UriHelper.GetPagingArgs(p, firstItem, lastItem),
                        toggleIsOn ? 1 : 0),
                GetReplies,
                "id");
        }

        /// <summary>整页一次反序列化。逐条 FromJson 会把每个节点再序列化一遍。</summary>
        private static IEnumerable<Entity> GetReplies(JsonArray array)
        {
            foreach (FeedReplyDto dto in DtoJson.DeserializeList<FeedReplyDto>(array))
            {
                if (dto == null) { continue; }
                yield return dto.EntityType == "feed_reply"
                    ? new FeedReplyModel(dto)
                    : new NullEntity();
            }
        }

        public void SetComboBoxSelectedIndex(int value)
        {
            switch (value)
            {
                case -1: return;
                case 0:
                    ReplyListType = "lastupdate_desc";
                    break;

                case 1:
                    ReplyListType = "dateline_desc";
                    break;

                case 2:
                    ReplyListType = "popular";
                    break;
            }
        }
    }
}
