using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    public class CollectionModel : Entity, IHasSubtitle
    {
        public int ID { get; private set; }
        public string Url { get; private set; }
        public int ItemNum { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public ImageModel Cover { get; private set; }
        public string Description { get; private set; }

        public ImageModel Pic => Cover;

        public CollectionModel(JsonObject token) : base(token)
        {
            if (token.TryGetPropertyValue("id", out JsonNode id))
            {
                ID = id.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("item_num", out JsonNode item_num))
            {
                ItemNum = item_num.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("subTitle", out JsonNode subTitle))
            {
                SubTitle = subTitle.ToString();
            }

            if (token.TryGetPropertyValue("url", out JsonNode url))
            {
                Url = url.ToString();
            }

            if (token.TryGetPropertyValue("description", out JsonNode description))
            {
                Description = description.ToString();
            }

            if (token.TryGetPropertyValue("cover_pic", out JsonNode cover_pic))
            {
                Cover = new ImageModel(cover_pic.ToString(), ImageType.OriginImage);
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }
}
