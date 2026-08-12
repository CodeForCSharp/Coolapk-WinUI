using CoolapkUWP.Common;
using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CoolapkUWP.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.Extensions.Logging;
using System.Text.Json;
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
        [ObservableProperty]
        public partial int LikeNum { get; set; }

        [ObservableProperty]
        public partial int ReplyNum { get; set; }

        [ObservableProperty]
        public partial int StarNum { get; set; }

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

        public bool Followed
        {
            get => UserAction.FollowAuthor;
            set
            {
                if (UserAction.FollowAuthor != value)
                {
                    UserAction.FollowAuthor = value;
                    OnPropertyChanged();
                }
            }
        }

        [ObservableProperty]
        public partial bool ShowButtons { get; set; }

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

        [ObservableProperty]
        public partial bool ShowLinkSourceFeed { get; set; }

        public string Info { get; private set; }
        public string VoteTag { get; private set; }
        public string InfoHTML { get; private set; }

        [ObservableProperty]
        public partial string ExtraUrl { get; set; }

        [ObservableProperty]
        public partial string ExtraSubtitle { get; set; }

        public string MediaUrl { get; private set; }
        public string IPLocation { get; private set; }
        public string ExtraTitle { get; private set; }
        public string DeviceTitle { get; private set; }
        public string VoteEndTime { get; private set; }
        public string VoteStartTime { get; private set; }
        public string MediaSubtitle { get; private set; }

        public ImageModel ExtraPic { get; private set; }
        public ImageModel MediaPic { get; private set; }
        public SourceFeedModel SourceFeed { get; private set; }

        [ObservableProperty]
        public partial LinkFeedModel LinkSourceFeed { get; set; }

        public List<VoteItem> VoteRows { get; private set; } = new List<VoteItem>();
        public List<RelationRowsItem> RelationRows { get; private set; } = new List<RelationRowsItem>();
        public List<SourceFeedReplyModel> ReplyRows { get; private set; } = new List<SourceFeedReplyModel>();

        public FeedModelBase(FeedDto dto) : base(dto)
        {
            LikeNum = dto.Likenum.ToInt32Safe();
            ReplyNum = dto.Replynum.ToInt32Safe();
            StarNum = dto.Favnum.ToInt32Safe();
            ShareNum = dto.Forwardnum.ToInt32Safe();

            Info = !string.IsNullOrEmpty(dto.Info)
                ? dto.Info
                : dto.FeedTypeName;

            InfoHTML = !string.IsNullOrEmpty(dto.InfoHtml)
                ? dto.InfoHtml
                : Dateline;

            if (IsVoteFeed && dto.Vote != null)
            {
                VoteDto vote = dto.Vote;

                TotalVoteNum = vote.TotalVoteNum.ToInt32Safe();
                TotalCommentNum = vote.TotalCommentNum.ToInt32Safe();
                VoteStartTime = vote.StartTime?.ToInt64Safe().ConvertUnixTimeStampToReadable(null);
                VoteEndTime = vote.EndTime?.ToInt64Safe().ConvertUnixTimeStampToReadable(null);
                VoteType = vote.Type.ToInt32Safe();
                VoteTag = vote.LinkTag;

                if (vote.Options != null)
                {
                    VoteRows = vote.Options.Select(item => new VoteItem(item)).ToList();
                }
            }

            if (IsQuestionFeed)
            {
                QuestionAnswerNum = dto.QuestionAnswerNum.ToInt32Safe();
                QuestionFollowNum = dto.QuestionFollowNum.ToInt32Safe();
            }

            DeviceTitle = !string.IsNullOrEmpty(dto.DeviceTitle)
                ? dto.DeviceTitle
                : dto.DeviceName;

            IPLocation = dto.IpLocation;

            if (!string.IsNullOrEmpty(dto.ExtraTitle))
            {
                ExtraTitle = dto.ExtraTitle;

                if (dto.ExtraUrl != null)
                {
                    ExtraUrl = dto.ExtraUrl;

                    if (ExtraUrl.Contains("b23.tv") || ExtraUrl.Contains("t.cn"))
                    {
                        _ = ExpandShortUrlAsync();
                    }
                    else
                    {
                        _ = BuildLinkSourceFeedAsync();
                    }

                    if (!string.IsNullOrEmpty(dto.ExtraPic))
                    {
                        ExtraPic = new ImageModel(dto.ExtraPic, ImageType.Icon);
                    }
                }
            }

            if (dto.MediaUrl != null)
            {
                MediaUrl = dto.MediaUrl;
                MediaSubtitle = MediaUrl.ValidateAndGetUri() is Uri ExtraUri && ExtraUri != null ? ExtraUri.Host : MediaUrl;

                if (!string.IsNullOrEmpty(dto.MediaPic))
                {
                    MediaPic = new ImageModel(dto.MediaPic, ImageType.Icon);
                }
            }

            ReplyRowsCount = dto.ReplyRowsCount.ToInt32Safe();

            if (dto.ReplyRows != null)
            {
                ReplyRows = dto.ReplyRows
                    .Select(item => new SourceFeedReplyModel(item))
                    .ToList();
            }

            string location = dto.Location;
            string ttitle = dto.Ttitle;
            string dyhName = dto.DyhName;
            int changeCount = dto.ChangeCount.ToInt32Safe();
            int status = dto.Status.ToInt32Safe();
            int blockStatus = dto.BlockStatus.ToInt32Safe();

            ShowRelationRows =
                (!string.IsNullOrEmpty(location)) |
                (!string.IsNullOrEmpty(ttitle)) |
                (!string.IsNullOrEmpty(dyhName)) |
                (dto.RelationRows is JsonArray relationRows && relationRows.Count > 0) |
                (changeCount > 0) |
                (status == -1) |
                (blockStatus != 0);

            if (ShowRelationRows)
            {
                List<RelationRowsItem> builder = new List<RelationRowsItem>();
                if (!string.IsNullOrEmpty(location))
                {
                    builder.Add(
                        new RelationRowsItem(
                            title: location,
                            icon: "\uE707"));
                }

                if (!string.IsNullOrEmpty(ttitle))
                {
                    builder.Add(
                        new RelationRowsItem(
                            url: dto.Turl,
                            title: ttitle,
                            logo: dto.Tpic));
                }

                if (EntityType != "article" && !string.IsNullOrEmpty(dyhName))
                {
                    builder.Add(
                        new RelationRowsItem(
                            url: $"/dyh/{dto.DyhId}",
                            title: dyhName));
                }

                if (dto.RelationRows is JsonArray relationRows2)
                {
                    foreach (JsonNode i in relationRows2)
                    {
                        JsonObject item = i.AsObject();
                        builder.Add(
                            new RelationRowsItem(
                                url: (string)item["url"],
                                title: (string)item["title"],
                                logo: (string)item["logo"]));
                    }
                }

                if (changeCount > 0)
                {
                    builder.Add(
                        new RelationRowsItem(
                            url: $"/feed/changeHistoryList?id={ID}",
                            title: $"已编辑{changeCount}次",
                            icon: "\uE70F"));
                }

                if (status == -1)
                {
                    builder.Add(
                        new RelationRowsItem(
                            title: "仅自己可见",
                            icon: "\uE727"));
                }

                if (blockStatus != 0)
                {
                    builder.Add(
                        new RelationRowsItem(
                            title: "已折叠",
                            icon: "\uE7BA"));
                }

                ShowRelationRows = builder.Any();
                RelationRows = builder;
            }

            if (!IsQuestionFeed
                && !string.IsNullOrEmpty(dto.SourceId))
            {
                ShowSourceFeed = true;
                if (dto.ForwardSourceFeed is JsonObject forwardSourceFeed)
                {
                    SourceFeed = SourceFeedModel.FromJson(forwardSourceFeed);
                }
                else
                {
                    EmptySourceFeed = true;
                }
            }
        }
        private async Task ExpandShortUrlAsync()
        {
            string expandedUrl = null;
            try
            {
                Uri uri = ExtraUrl.ValidateAndGetUri();
                if (uri != null)
                {
                    expandedUrl = await uri.ExpandShortUrlAsync();
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(FeedModelBase)).LogWarning(ex, ex.ExceptionToMessage());
            }

            await WindowContext.DispatcherQueue.ResumeForegroundAsync();
            if (!string.IsNullOrEmpty(expandedUrl))
            {
                ExtraUrl = expandedUrl;
            }
            _ = BuildLinkSourceFeedAsync();
        }

        private async Task BuildLinkSourceFeedAsync()
        {
            ExtraSubtitle = ExtraUrl.ValidateAndGetUri() is Uri ExtraUri && ExtraUri != null ? ExtraUri.Host : ExtraUrl;

            if (ExtraUrl.Contains("coolapk") && ExtraUrl.Contains("feed"))
            {
                LinkSourceFeed = new LinkFeedModel(await LinkPreviewService.LoadAsync(new Uri(ExtraUrl), LinkType.Coolapk), LinkType.Coolapk);
                ShowLinkSourceFeed = true;
            }
            else if (ExtraUrl.Contains("bilibili") && ExtraUrl.Contains("t.bilibili"))
            {
                Regex GetID = new Regex(@"/t.*?/([\d|\w]+)");
                Uri uri = UriHelper.GetLinkUri(UriType.GetBilibiliFeed, LinkType.Bilibili, GetID.Match(ExtraUrl).Groups[1].Value);
                MultipartFormDataContent content = new MultipartFormDataContent { { new StringContent(GetID.Match(ExtraUrl).Groups[1].Value), "dynamic_id" } };
                LinkSourceFeed = new LinkFeedModel(await LinkPreviewService.LoadAsync(uri, LinkType.Bilibili, true, content), LinkType.Bilibili);
                ShowLinkSourceFeed = true;
            }
            else if (ExtraUrl.Contains("ithome") && ExtraUrl.Contains("qcontent"))
            {
                Regex GetID = new Regex(@"[%26|%3F]id%3D([\d|\w]+)");
                Uri uri = UriHelper.GetLinkUri(UriType.GetITHomeFeed, LinkType.ITHome, GetID.Match(ExtraUrl).Groups[1].Value);
                LinkSourceFeed = new LinkFeedModel(await LinkPreviewService.LoadAsync(uri, LinkType.ITHome), LinkType.ITHome);
                ShowLinkSourceFeed = true;
            }
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

        public VoteItem(VoteItemDto dto)
        {
            ID = dto.Id.ToInt32Safe();
            Order = dto.Order.ToInt32Safe();
            VoteID = dto.VoteId.ToInt32Safe();
            Status = dto.Status.ToInt32Safe();
            Title = dto.Title;

            if (!string.IsNullOrEmpty(dto.Color))
            {
                try
                {
                    Color = dto.Color.ToColor();
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
