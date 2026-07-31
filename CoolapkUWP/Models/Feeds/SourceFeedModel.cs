using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using System.Text.Json.Nodes;
using System.Collections.Generic;

using System.Linq;
using System.ComponentModel;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Feeds
{
    public partial class SourceFeedModel : Entity, INotifyPropertyChanged
    {
        private bool showUser = true;
        public bool ShowUser
        {
            get => showUser;
            set
            {
                if (showUser != value)
                {
                    showUser = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isCopyEnabled;
        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                if (isCopyEnabled != value)
                {
                    isCopyEnabled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public int RatingStar { get; private set; }

        public bool IsVoteFeed { get; private set; }
        public bool IsRatingFeed { get; private set; }
        public bool IsCoolPicture { get; private set; }
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

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public SourceFeedModel(JsonObject token) : base(token)
        {
            if (token.TryGetPropertyValue("url", out JsonNode uri) && !string.IsNullOrEmpty(uri.ToString()))
            {
                Url = uri.ToString();
            }
            else if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                Url = $"/feed/{id.ToString().Replace("\"", string.Empty)}";
            }

            if (token.TryGetPropertyValue("userInfo", out JsonNode v1))
            {
                JsonObject userInfo = v1.AsObject();
                UserInfo = new UserModel(userInfo);
            }
            else
            {
                UserInfo = new UserModel(null);
            }

            if (token.TryGetPropertyValue("userAction", out JsonNode v2))
            {
                JsonObject userAction = v2.AsObject();
                UserAction = new UserAction(userAction);
            }
            else
            {
                UserAction = new UserAction(null);
            }

            ShareUrl = token.TryGetPropertyValue("shareUrl", out JsonNode shareUrl) && !string.IsNullOrEmpty(shareUrl.ToString())
                ? shareUrl.ToString()
                : $"https://www.coolapk.com{(Url != null ? Url.Replace("/question/", "/feed/") : string.Empty)}";

            if (token.TryGetPropertyValue("message", out JsonNode message))
            {
                ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");
                Message = message.ToString().Replace("<a href=\"\">查看更多</a>", $"<a href=\"{Url}\">{loader.GetString("ReadMore")}</a>");
            }

            if (token.TryGetPropertyValue("message_title", out JsonNode message_title))
            {
                MessageTitle = message_title.ToString();
            }

            if (token.TryGetPropertyValue("feedType", out JsonNode feedType))
            {
                FeedType = feedType.ToString();
                switch (FeedType)
                {
                    case "vote":
                        IsVoteFeed = true;
                        Url = Url.Replace("/feed/", "/vote/");
                        break;
                    case "rating":
                        IsRatingFeed = true;
                        if (token.TryGetPropertyValue("star", out JsonNode star))
                        {
                            RatingStar = star.ToInt32Safe();
                        }
                        break;
                    case "question":
                        IsQuestionFeed = true;
                        Url = Url.Replace("/feed/", "/question/");
                        break;
                }
            }

            if (token.TryGetPropertyValue("dateline", out JsonNode dateline))
            {
                Dateline = dateline.ToInt64Safe().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetPropertyValue("picArr", out JsonNode picArr) && picArr.AsArray().Count > 0)
            {
                PicArr = picArr.AsArray().Select(
                    x => !string.IsNullOrEmpty(x.ToString())
                        ? new ImageModel(x.ToString(), ImageType.SmallImage) : null)
                    .Where(x => x != null).ToList();

                foreach (ImageModel item in PicArr)
                {
                    item.ContextArray = PicArr;
                }
            }

            if (token.TryGetPropertyValue("pic", out JsonNode pic) && !string.IsNullOrEmpty(pic.ToString()))
            {
                Pic = new ImageModel(pic.ToString(), ImageType.SmallImage);
            }
        }

        public override string ToString() => Message;
    }
}
