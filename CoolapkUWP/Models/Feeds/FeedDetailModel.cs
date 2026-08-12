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
            ReadNum = dto.ReadNum.ToInt32Safe();
            Title = dto.Title;

            if (dto.TargetRow is JsonObject v)
            {
                ShowDyhName = true;

                DyhLogo = v.TryGetPropertyValue("logo", out JsonNode logo) ? new ImageModel(logo.ToString(), ImageType.Icon) : null;
                DyhName = v.TryGetPropertyValue("title", out JsonNode dtitle) ? dtitle.ToString() : null;
                DyhUrl = v.TryGetPropertyValue("url", out JsonNode url) ? url.ToString() : null;
                DyhSubTitle = v.TryGetPropertyValue("subTitle", out JsonNode subTitle) ? subTitle.ToString() : null;
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
                        if (dto.ExtraData is JsonNode extraData)
                        {
                            JsonObject j = JsonNode.Parse(extraData.ToJsonString()).AsObject();
                            QuestionUrl = j.TryGetPropertyValue("questionUrl", out JsonNode questionUrl)
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
            => new FeedDetailModel(JsonSerializer.Deserialize<FeedDto>(json, DtoJson.Options));

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
