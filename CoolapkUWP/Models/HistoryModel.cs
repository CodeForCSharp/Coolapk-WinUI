using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    public class HistoryModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public ImageModel Pic { get; private set; }
        public string Description { get; private set; }

        public HistoryModel(JsonObject token) : base(token)
        {
            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("url", out JsonNode url))
            {
                Url = url.ToString();
            }

            if (token.TryGetPropertyValue("description", out JsonNode description))
            {
                Description = description.ToString();
            }
            else if (token.TryGetPropertyValue("target_type_title", out JsonNode target_type_title) && !string.IsNullOrEmpty(target_type_title.ToString()))
            {
                Description = target_type_title.ToString();
            }
            else if (token.TryGetPropertyValue("dateline", out JsonNode dateline))
            {
                Description = dateline.ToInt64Safe().ConvertUnixTimeStampToReadable();
            }

            if (token.TryGetPropertyValue("logo", out JsonNode logo))
            {
                Pic = new ImageModel(logo.ToString(), ImageType.Icon);
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }
}
