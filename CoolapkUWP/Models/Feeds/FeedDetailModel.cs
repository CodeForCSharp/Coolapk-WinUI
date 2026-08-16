using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json;
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

        public FeedDetailModel(FeedDto dto) : base(dto)
        {
            ReadNum = dto.ReadNum;
            Title = dto.Title;

            if (dto.TargetRow != null)
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
