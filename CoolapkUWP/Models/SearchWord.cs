using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    public class SearchWord : Entity
    {
        public string Glyph { get; set; }
        public string Title { get; set; }

        public SearchWord(JsonObject keys) : base(keys)
        {
            if (keys.TryGetPropertyValue("logo", out JsonNode logo))
            {
                Glyph = logo.ToString().Contains("app") || logo.ToString().Contains("cube")
                    ? "\uE719"
                    : logo.ToString().Contains("xitongguanli") ? "\uE77B" : "\uE721";
            }

            if (keys.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }
        }

        public override string ToString()
        {
            switch (Glyph)
            {
                case "\uE719":
                case "\uE77B":
                    return Title
                        .Replace("搜索应用：", string.Empty)
                        .Replace("搜索用户：", string.Empty);
                default:
                    return Title;
            }
        }
    }
}
