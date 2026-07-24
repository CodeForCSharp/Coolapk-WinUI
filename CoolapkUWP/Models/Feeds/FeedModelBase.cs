using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CommunityToolkit.WinUI.Helpers;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.UI;
using Windows.UI;

namespace CoolapkUWP.Models.Feeds
{
    public partial class FeedModelBase : SourceFeedModel, ICanFollow, ICanLike, ICanReply, ICanStar
    {
        private int likeNum;
        public int LikeNum
        {
            get => likeNum;
            set
            {
                if (likeNum != value)
                {
                    likeNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private int replyNum;
        public int ReplyNum
        {
            get => replyNum;
            set
            {
                if (replyNum != value)
                {
                    replyNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private int starNum;
        public int StarNum
        {
            get => starNum;
            set
            {
                if (starNum != value)
                {
                    starNum = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool Liked
        {
            get => UserAction.Like;
            set
            {
                if (UserAction.Like != value)
                {
                    UserAction.Like = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public bool Followed
        {
            get => UserAction.FollowAuthor;
            set
            {
                if (UserAction.FollowAuthor != value)
                {
                    UserAction.FollowAuthor = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool showButtons = true;
        public bool ShowButtons
        {
            get => showButtons;
            set
            {
                if (showButtons != value)
                {
                    showButtons = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        int ICanFollow.ID => UID;

        public int ID => EntityID;
        public int UID => UserInfo.UID;
        public int VoteType { get; private set; }
        public int ShareNum { get; private set; }
        public int TotalVoteNum { get; private set; }
        public int ReplyRowsCount { get; private set; }
        public int TotalCommentNum { get; private set; }
        public int QuestionAnswerNum { get; private set; }
        public int QuestionFollowNum { get; private set; }

        public bool Stared { get; set; }
        public bool ShowSourceFeed { get; private set; }
        public bool EmptySourceFeed { get; private set; }
        public bool ShowRelationRows { get; private set; }
        public bool ShowLinkSourceFeed { get; private set; }

        public string Info { get; private set; }
        public string VoteTag { get; private set; }
        public string InfoHTML { get; private set; }
        public string ExtraUrl { get; private set; }
        public string MediaUrl { get; private set; }
        public string IPLocation { get; private set; }
        public string ExtraTitle { get; private set; }
        public string DeviceTitle { get; private set; }
        public string VoteEndTime { get; private set; }
        public string VoteStartTime { get; private set; }
        public string ExtraSubtitle { get; private set; }
        public string MediaSubtitle { get; private set; }

        public ImageModel ExtraPic { get; private set; }
        public ImageModel MediaPic { get; private set; }
        public SourceFeedModel SourceFeed { get; private set; }
        public LinkFeedModel LinkSourceFeed { get; private set; }

        public List<VoteItem> VoteRows { get; private set; } = new List<VoteItem>();
        public List<RelationRowsItem> RelationRows { get; private set; } = new List<RelationRowsItem>();
        public List<SourceFeedReplyModel> ReplyRows { get; private set; } = new List<SourceFeedReplyModel>();

        public FeedModelBase(JsonObject token) : base(token)
        {
            if (token.TryGetPropertyValue("info", out JsonNode info) && !string.IsNullOrEmpty(info.ToString()))
            {
                Info = info.ToString();
            }
            else if (token.TryGetPropertyValue("feedTypeName", out JsonNode feedTypeName))
            {
                Info = feedTypeName.ToString();
            }

            InfoHTML = token.TryGetPropertyValue("infoHtml", out JsonNode infoHtml) && !string.IsNullOrEmpty(infoHtml.ToString())
                ? infoHtml.ToString()
                : Dateline;

            if (token.TryGetPropertyValue("likenum", out JsonNode likenum))
            {
                LikeNum = likenum.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("favnum", out JsonNode favnum))
            {
                StarNum = favnum.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("replynum", out JsonNode replynum))
            {
                ReplyNum = replynum.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("forwardnum", out JsonNode forwardnum))
            {
                ShareNum = forwardnum.ToInt32Safe();
            }

            if (IsVoteFeed)
            {
                if (token.TryGetPropertyValue("vote", out JsonNode v))
                {
                    JsonObject vote = v.AsObject();
                    if (vote.TryGetPropertyValue("total_vote_num", out JsonNode total_vote_num))
                    {
                        TotalVoteNum = total_vote_num.ToInt32Safe();
                    }

                    if (vote.TryGetPropertyValue("total_comment_num", out JsonNode total_comment_num))
                    {
                        TotalCommentNum = total_comment_num.ToInt32Safe();
                    }

                    if (vote.TryGetPropertyValue("start_time", out JsonNode start_time))
                    {
                        VoteStartTime = start_time.ToInt64Safe().ConvertUnixTimeStampToReadable(null);
                    }

                    if (vote.TryGetPropertyValue("end_time", out JsonNode end_time))
                    {
                        VoteEndTime = end_time.ToInt64Safe().ConvertUnixTimeStampToReadable(null);
                    }

                    if (vote.TryGetPropertyValue("type", out JsonNode type))
                    {
                        VoteType = type.ToInt32Safe();
                    }

                    if (vote.TryGetPropertyValue("link_tag", out JsonNode link_tag))
                    {
                        VoteTag = link_tag.ToString();
                    }

                    if (vote.TryGetPropertyValue("options", out JsonNode options))
                    {
                        VoteRows = options.AsArray().Select(item => new VoteItem(item.AsObject())).ToList();
                    }
                }
            }

            if (IsQuestionFeed)
            {
                if (token.TryGetPropertyValue("question_answer_num", out JsonNode question_answer_num))
                {
                    QuestionAnswerNum = question_answer_num.ToInt32Safe();
                }
                if (token.TryGetPropertyValue("question_follow_num", out JsonNode question_follow_num))
                {
                    QuestionFollowNum = question_follow_num.ToInt32Safe();
                }
            }

            if (token.TryGetPropertyValue("device_title", out JsonNode device_title) && !string.IsNullOrEmpty(device_title.ToString()))
            {
                DeviceTitle = device_title.ToString();
            }
            else if (token.TryGetPropertyValue("device_name", out JsonNode device_name))
            {
                DeviceTitle = device_name.ToString();
            }

            if (token.TryGetPropertyValue("ip_location", out JsonNode ip_location))
            {
                IPLocation = ip_location.ToString();
            }

            if (token.TryGetPropertyValue("extra_title", out JsonNode extra_title) && !string.IsNullOrEmpty(extra_title.ToString()))
            {
                ExtraTitle = extra_title.ToString();

                if (token.TryGetPropertyValue("extra_url", out JsonNode extra_url))
                {
                    ExtraUrl = extra_url.ToString();

                    if (ExtraUrl.Contains("b23.tv") || ExtraUrl.Contains("t.cn"))
                    {
                        ExtraUrl = ExtraUrl.ValidateAndGetUri().ExpandShortUrl();
                    }

                    ExtraSubtitle = ExtraUrl.ValidateAndGetUri() is Uri ExtraUri && ExtraUri != null ? ExtraUri.Host : ExtraUrl;

                    if (token.TryGetPropertyValue("extra_pic", out JsonNode extra_pic))
                    {
                        ExtraPic = new ImageModel(extra_pic.ToString(), ImageType.Icon);
                    }

                    if (ExtraUrl.Contains("coolapk") && ExtraUrl.Contains("feed"))
                    {
                        LinkSourceFeed = new LinkFeedModel(new Uri(ExtraUrl), LinkType.Coolapk);
                        ShowLinkSourceFeed = true;
                    }
                    else if (ExtraUrl.Contains("bilibili") && ExtraUrl.Contains("t.bilibili"))
                    {
                        Regex GetID = new Regex(@"/t.*?/([\d|\w]+)");
                        Uri uri = UriHelper.GetLinkUri(UriType.GetBilibiliFeed, LinkType.Bilibili, GetID.Match(ExtraUrl).Groups[1].Value);
                        MultipartFormDataContent content = new MultipartFormDataContent { { new StringContent(GetID.Match(ExtraUrl).Groups[1].Value), "dynamic_id" } };
                        LinkSourceFeed = new LinkFeedModel(uri, LinkType.Bilibili, true, content);
                        ShowLinkSourceFeed = true;
                    }
                    else if (ExtraUrl.Contains("ithome") && ExtraUrl.Contains("qcontent"))
                    {
                        Regex GetID = new Regex(@"[%26|%3F]id%3D([\d|\w]+)");
                        Uri uri = UriHelper.GetLinkUri(UriType.GetITHomeFeed, LinkType.ITHome, GetID.Match(ExtraUrl).Groups[1].Value);
                        LinkSourceFeed = new LinkFeedModel(uri, LinkType.ITHome);
                        ShowLinkSourceFeed = true;
                    }
                }
            }

            if (token.TryGetPropertyValue("media_url", out JsonNode media_url))
            {
                MediaUrl = media_url.ToString();
                MediaSubtitle = MediaUrl.ValidateAndGetUri() is Uri ExtraUri && ExtraUri != null ? ExtraUri.Host : MediaUrl;

                if (token.TryGetPropertyValue("media_pic", out JsonNode media_pic))
                {
                    MediaPic = new ImageModel(media_pic.ToString(), ImageType.Icon);
                }
            }

            if (token.TryGetPropertyValue("replyRowsCount", out JsonNode replyRowsCount))
            {
                ReplyRowsCount = replyRowsCount.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("replyRows", out JsonNode replyRows))
            {
                ReplyRows = replyRows.AsArray().Select(item => new SourceFeedReplyModel(item.AsObject())).ToList();
            }

            ShowRelationRows =
                (token.TryGetPropertyValue("location", out JsonNode location) && !string.IsNullOrEmpty(location.ToString())) |
                (token.TryGetPropertyValue("ttitle", out JsonNode ttitle) && !string.IsNullOrEmpty(ttitle.ToString())) |
                (token.TryGetPropertyValue("dyh_name", out JsonNode dyh_name) && !string.IsNullOrEmpty(dyh_name.ToString())) |
                (token.TryGetPropertyValue("relationRows", out JsonNode relationRows) && relationRows.AsArray().Count > 0) |
                (token.TryGetPropertyValue("change_count", out JsonNode change_count) && change_count.ToInt32Safe() > 0) |
                (token.TryGetPropertyValue("status", out JsonNode status) && status.ToInt32Safe() == -1) |
                (token.TryGetPropertyValue("block_status", out JsonNode block_status) && block_status.ToInt32Safe() != 0);

            if (ShowRelationRows)
            {
                List<RelationRowsItem> buider = new List<RelationRowsItem>();
                if (location != null && !string.IsNullOrEmpty(location.ToString()))
                {
                    buider.Add(
                        new RelationRowsItem(
                            title: location.ToString(),
                            icon: "\uE707"));
                }

                if (ttitle != null && !string.IsNullOrEmpty(ttitle.ToString()))
                {
                    buider.Add(
                        new RelationRowsItem(
                            url: (string)token["turl"],
                            title: ttitle.ToString(),
                            logo: (string)token["tpic"]));
                }

                if (EntityType != "article" && dyh_name != null && !string.IsNullOrEmpty(dyh_name.ToString()))
                {
                    buider.Add(
                        new RelationRowsItem(
                            url: $"/dyh/{token["dyh_id"]}",
                            title: dyh_name.ToString()));
                }

                if (relationRows != null)
                {
                    foreach (JsonNode i in relationRows.AsArray())
                    {
                        JsonObject item = i.AsObject();
                        buider.Add(
                            new RelationRowsItem(
                                url: (string)item["url"],
                                title: (string)item["title"],
                                logo: (string)item["logo"]));
                    }
                }

                if (change_count != null && change_count.ToInt32Safe() > 0)
                {
                    buider.Add(
                        new RelationRowsItem(
                            url: $"/feed/changeHistoryList?id={ID}",
                            title: $"已编辑{change_count.ToInt32Safe()}次",
                            icon: "\uE70F"));
                }

                if (status != null && status.ToInt32Safe() == -1)
                {
                    buider.Add(
                        new RelationRowsItem(
                            title: "仅自己可见",
                            icon: "\uE727"));
                }

                if (block_status != null && block_status.ToInt32Safe() != 0)
                {
                    buider.Add(
                        new RelationRowsItem(
                            title: "已折叠",
                            icon: "\uE7BA"));
                }

                ShowRelationRows = buider.Any();
                RelationRows = buider;
            }

            if (!IsQuestionFeed
                && token.TryGetPropertyValue("source_id", out JsonNode source_id)
                && !string.IsNullOrEmpty(source_id.ToString()))
            {
                ShowSourceFeed = true;
                if (token.TryGetPropertyValue("forwardSourceFeed", out JsonNode forwardSourceFeed)
                    && !string.IsNullOrEmpty(forwardSourceFeed.ToString())
                    && forwardSourceFeed.ToString() != "null")
                {
                    SourceFeed = new SourceFeedModel(forwardSourceFeed.AsObject());
                }
                else
                {
                    EmptySourceFeed = true;
                }
            }
        }

        public async Task ChangeLike()
        {
            UriType type = Liked ? UriType.PostFeedUnlike : UriType.PostFeedLike;
            (bool isSucceed, JsonNode result) = await RequestHelper.PostDataAsync(UriHelper.GetOldUri(type, string.Empty, ID), null, true);
            if (!isSucceed) { return; }
            Liked = !Liked;
            if (result.AsObject().TryGetPropertyValue("count", out JsonNode count))
            {
                LikeNum = count.ToInt32Safe();
            }
        }

        public async Task ChangeFollow()
        {
            UriType type = Followed ? UriType.PostUserUnfollow : UriType.PostUserFollow;

            (bool isSucceed, _) = await RequestHelper.PostDataAsync(UriHelper.GetUri(type, UID), null, true);
            if (!isSucceed) { return; }

            Followed = !Followed;
        }
    }

    public class VoteItem
    {
        public int ID { get; set; }
        public int Order { get; set; }
        public int VoteID { get; set; }
        public int Status { get; set; }

        public string Title { get; set; }

        public Color Color { get; set; }

        public VoteItem(JsonObject token)
        {
            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("order", out JsonNode order))
            {
                Order = order.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("vote_id", out JsonNode vote_id))
            {
                VoteID = vote_id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("status", out JsonNode status))
            {
                Status = status.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("color", out JsonNode color))
            {
                if (!string.IsNullOrEmpty(color.ToString()))
                {
                    try
                    {
                        Color = color.ToString().ToColor();
                    }
                    catch
                    {
                        Color = Colors.Transparent;
                    }
                }
                else
                {
                    Color = Colors.Transparent;
                }
            }
        }
    }

    public class RelationRowsItem
    {
        public string Url { get; set; }
        public string Title { get; set; }

        public string Icon { get; set; }
        public ImageModel Logo { get; set; }

        public bool IsShowLogo => Logo != null;
        public bool IsShowIcon => Logo != null || !string.IsNullOrWhiteSpace(Icon);

        public RelationRowsItem(string url = null, string title = null, string icon = null, string logo = null)
        {
            Url = url;
            Title = title;
            Icon = icon;
            if (logo != null)
            {
                Logo = new ImageModel(logo, ImageType.Icon);
            }
        }
    }
}
