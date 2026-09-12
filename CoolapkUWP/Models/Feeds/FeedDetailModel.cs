using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text;

namespace CoolapkUWP.Models.Feeds
{
    public partial class FeedDetailModel : FeedModelBase
    {
        public int ReadNum { get; private set; }

        public bool ShowDyhName { get; private set; }
        public bool IsAnswerFeed { get; private set; }
        public bool IsFeedArticle { get; private set; }
        public bool ShowTopicTitle { get; private set; }
        public bool ShowRelationCards { get; private set; }
        public bool ShowChipRelationRows { get; private set; }
        public bool HasSinglePic { get; private set; }
        public bool HasMultiplePics { get; private set; }
        public bool ShowMessageTitle { get; private set; }

        public string Title { get; private set; }
        public string DyhUrl { get; private set; }
        public string DyhName { get; private set; }
        public string TopicUrl { get; private set; }
        public string TopicTitle { get; private set; }
        public string DyhSubTitle { get; private set; }
        public string QuestionUrl { get; private set; }
        public string MessageRawOutput { get; private set; }

        public ImageModel DyhLogo { get; private set; }
        public ImageModel TopicLogo { get; private set; }
        public ImageModel MessageCover { get; private set; }
        public ImageModel FirstPic { get; private set; }

        public FeedRelationCard ProductCard { get; private set; }
        public List<FeedRelationCard> RelationCards { get; private set; } = new List<FeedRelationCard>();
        public List<RelationRowsItem> ChipRelationRows { get; private set; } = new List<RelationRowsItem>();

        public FeedDetailModel(FeedDto dto) : base(dto)
        {
            ReadNum = dto.ReadNum;
            Title = dto.Title;

            if (dto.TargetRow != null)
            {
                if (FeedRelationCard.IsProductLink(dto.TargetRow.Url, targetType: dto.TargetRow.TargetType))
                {
                    TryAddRelationCard(FeedRelationCard.FromTargetRow(dto.TargetRow, FeedRelationKind.Product));
                }
                else if (FeedRelationCard.IsTopicLink(dto.TargetRow.Url, targetType: dto.TargetRow.TargetType))
                {
                    TryAddRelationCard(FeedRelationCard.FromTargetRow(dto.TargetRow, FeedRelationKind.Topic));
                }
                else
                {
                    ShowDyhName = true;

                    if (!string.IsNullOrEmpty(dto.TargetRow.Logo))
                    {
                        DyhLogo = new ImageModel(dto.TargetRow.Logo, ImageType.Icon);
                    }
                    DyhName = dto.TargetRow.Title;
                    DyhUrl = dto.TargetRow.Url;
                    DyhSubTitle = dto.TargetRow.SubTitle;
                }
            }

            if (!string.IsNullOrEmpty(dto.Ttitle) && !ShowDyhName)
            {
                ShowTopicTitle = true;
                TopicTitle = dto.Ttitle;
                TopicUrl = dto.Turl;

                if (!string.IsNullOrEmpty(dto.Tpic))
                {
                    TopicLogo = new ImageModel(dto.Tpic, ImageType.Icon);
                }
            }

            if (EntityType != "article")
            {
                switch (FeedType)
                {
                    case "answer":
                        IsAnswerFeed = true;
                        if (dto.ExtraData is JsonObject extraData)
                        {
                            QuestionUrl = extraData.TryGetPropertyValue("questionUrl", out JsonNode questionUrl)
                                ? questionUrl.ToString() : null;
                        }

                        MessageRawOutput = BuildMessageRawOutput(dto.MessageRawOutput, "uri", false);
                        break;

                    case "feedArticle":
                        IsFeedArticle = true;
                        if (!string.IsNullOrEmpty(dto.MessageCover))
                        {
                            MessageCover = new ImageModel(dto.MessageCover, ImageType.SmallImage);
                        }

                        MessageRawOutput = BuildMessageRawOutput(dto.MessageRawOutput, "url", true);
                        break;
                }
            }

            foreach (RelationRowsItem item in RelationRows)
            {
                if (item.IsProduct || item.IsTopic)
                {
                    TryAddRelationCard(FeedRelationCard.FromRelation(item));
                }
            }

            if (!string.IsNullOrEmpty(TopicTitle))
            {
                TryAddRelationCard(
                    new FeedRelationCard(
                        FeedRelationKind.Topic,
                        0,
                        TopicUrl,
                        TopicTitle,
                        TopicLogo));
            }

            foreach (FeedRelationCard card in RelationCards)
            {
                card.EnsureDetailLoaded();
            }

            ShowRelationCards = RelationCards.Count > 0;
            ProductCard = RelationCards.Find(card => card.IsProduct);

            ChipRelationRows = RelationRows.FindAll(item => !IsShownAsLargeCard(item));
            ShowChipRelationRows = ChipRelationRows.Count > 0;

            bool showPicArr = string.IsNullOrEmpty(MessageRawOutput) && PicArr != null && PicArr.Count > 0;
            HasSinglePic = showPicArr && PicArr.Count == 1;
            HasMultiplePics = showPicArr && PicArr.Count > 1;
            FirstPic = HasSinglePic ? PicArr[0] : null;
            ShowMessageTitle = !IsFeedArticle && !string.IsNullOrEmpty(MessageTitle);
        }

        private void TryAddRelationCard(FeedRelationCard card)
        {
            if (card == null || (string.IsNullOrEmpty(card.Title) && string.IsNullOrEmpty(card.Url)))
            {
                return;
            }

            if (RelationCards.Exists(item => SameLink(item.Url, card.Url) || SameTitle(item.Title, card.Title)))
            {
                return;
            }

            RelationCards.Add(card);
        }

        private bool IsShownAsLargeCard(RelationRowsItem item)
        {
            if (item == null) { return false; }

            if (RelationCards.Exists(card => SameLink(item.Url, card.Url) || SameTitle(item.Title, card.Title)))
            {
                return true;
            }

            if (item.IsProduct && RelationCards.Exists(card => card.IsProduct))
            {
                return true;
            }

            if (ShowDyhName && (SameLink(item.Url, DyhUrl) || SameTitle(item.Title, DyhName)))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(ExtraTitle)
                && (SameLink(item.Url, ExtraUrl) || SameTitle(item.Title, ExtraTitle)))
            {
                return true;
            }

            return false;
        }

        private static bool SameLink(string left, string right)
        {
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right)) { return false; }
            return string.Equals(left.TrimEnd('/'), right.TrimEnd('/'), System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool SameTitle(string left, string right)
        {
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right)) { return false; }
            return string.Equals(left, right, System.StringComparison.OrdinalIgnoreCase);
        }

        public static new FeedDetailModel FromJson(JsonObject json)
            => new FeedDetailModel(DtoJson.Deserialize<FeedDto>(json));

        private static string BuildMessageRawOutput(string raw, string imageField, bool articleStyle)
        {
            if (string.IsNullOrEmpty(raw)) { return string.Empty; }

            StringBuilder builder = new StringBuilder();
            foreach (JsonNode item in JsonNode.Parse(raw).AsArray())
            {
                JsonObject itemObj = item.AsObject();
                if (itemObj.TryGetPropertyValue("type", out JsonNode type))
                {
                    switch (type.ToString())
                    {
                        case "text":
                            if (itemObj.TryGetPropertyValue("message", out JsonNode message))
                            {
                                builder.Append(message.ToString());
                            }
                            break;

                        case "image":
                            if (itemObj.TryGetPropertyValue(imageField, out JsonNode uri))
                            {
                                itemObj.TryGetPropertyValue("description", out JsonNode description);
                                builder.Append(articleStyle
                                    ? $"\n<img src=\"{uri}\" alt=\"{description}\"/>\n"
                                    : $"\n<img src=\"{uri}\" alt=\"{description}\">{description}</a>\n");
                            }
                            break;
                    }
                }
            }
            return builder.ToString();
        }
    }
}
