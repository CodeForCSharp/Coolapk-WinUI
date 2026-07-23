using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json.Nodes;
using System.Text;

namespace CoolapkUWP.Models.Feeds
{
    public class FeedDetailModel : FeedModelBase
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

        public FeedDetailModel(JsonObject token) : base(token)
        {
            if (token.TryGetPropertyValue("readNum", out JsonNode readNum))
            {
                ReadNum = readNum.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("targetRow", out JsonNode v))
            {
                ShowDyhName = true;

                JsonObject targetRow = v.AsObject();

                if (targetRow.TryGetPropertyValue("logo", out JsonNode logo))
                {
                    DyhLogo = new ImageModel(logo.ToString(), ImageType.Icon);
                }

                if (targetRow.TryGetPropertyValue("title", out JsonNode dtitle))
                {
                    DyhName = dtitle.ToString();
                }

                if (targetRow.TryGetPropertyValue("url", out JsonNode url))
                {
                    DyhUrl = url.ToString();
                }

                if (targetRow.TryGetPropertyValue("subTitle", out JsonNode subTitle))
                {
                    DyhSubTitle = subTitle.ToString();
                }
            }

            if (token.TryGetPropertyValue("ttitle", out JsonNode ttitle) && !ShowDyhName && !string.IsNullOrEmpty(ttitle.ToString()))
            {
                ShowTopicTitle = true;

                TopicTitle = ttitle.ToString();

                if (token.TryGetPropertyValue("turl", out JsonNode turl))
                {
                    TopicUrl = turl.ToString();
                }

                if (token.TryGetPropertyValue("tpic", out JsonNode tpic))
                {
                    TopicLogo = new ImageModel(tpic.ToString(), ImageType.Icon);
                }
            }

            if (EntityType != "article")
            {
                switch (FeedType)
                {
                    case "answer":
                        IsAnswerFeed = true;
                        if (token.TryGetPropertyValue("extraData", out JsonNode extraData))
                        {
                            JsonObject j = JsonNode.Parse(extraData.ToString()).AsObject();
                            if (j.TryGetPropertyValue("questionUrl", out JsonNode questionUrl))
                            {
                                QuestionUrl = questionUrl.ToString();
                            }
                        }

                        MessageRawOutput = string.Empty;
                        StringBuilder builder = new StringBuilder();
                        if (token.TryGetPropertyValue("message_raw_output", out JsonNode message_raw_output))
                        {
                            foreach (JsonNode item in JsonNode.Parse(message_raw_output.ToString()).AsArray())
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
                                            if (itemObj.TryGetPropertyValue("uri", out JsonNode uri))
                                            {
                                                itemObj.TryGetPropertyValue("description", out JsonNode description);
                                                builder.Append($"\n<img src=\"{uri}\" alt=\"{description}\">{description}</a>\n");
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                        MessageRawOutput = builder.ToString();
                        break;

                    case "feedArticle":
                        IsFeedArticle = true;
                        if (token.TryGetPropertyValue("message_cover", out JsonNode message_cover) && !string.IsNullOrEmpty(message_cover.ToString()))
                        {
                            MessageCover = new ImageModel(message_cover.ToString(), ImageType.SmallImage);
                        }

                        MessageRawOutput = string.Empty;
                        builder = new StringBuilder();
                        if (token.TryGetPropertyValue("message_raw_output", out message_raw_output))
                        {
                            foreach (JsonNode item in JsonNode.Parse(message_raw_output.ToString()).AsArray())
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
                                            if (itemObj.TryGetPropertyValue("url", out JsonNode uri))
                                            {
                                                itemObj.TryGetPropertyValue("description", out JsonNode description);
                                                builder.Append($"\n<img src=\"{uri}\" alt=\"{description}\"/>\n");
                                            }
                                            break;
                                    }
                                }
                            }
                        }
                        MessageRawOutput = builder.ToString();
                        break;
                }
            }
        }
    }
}
