using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    public class AppModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string FollowNum { get; private set; }
        public string DownloadNum { get; private set; }
        public string VersionCode { get; private set; }
        public string VersionName { get; private set; }
        public string Description { get; private set; }
        public string LastUpdate { get; private set; }
        public ImageModel Logo { get; private set; }

        public ImageModel Pic => Logo;

        public AppModel(JsonObject token) : base(token)
        {
            if (token.TryGetPropertyValue("url", out JsonNode url))
            {
                Url = url.ToString();
            }

            if (token.TryGetPropertyValue("followCount", out JsonNode followCount))
            {
                FollowNum = followCount.ToString();
            }

            if (token.TryGetPropertyValue("downCount", out JsonNode downCount))
            {
                DownloadNum = downCount.ToString();
            }

            if (token.TryGetPropertyValue("apkversioncode", out JsonNode apkversioncode))
            {
                VersionCode = apkversioncode.ToString();
            }

            if (token.TryGetPropertyValue("apkversionname", out JsonNode apkversionname))
            {
                VersionName = apkversionname.ToString();
            }

            if (token.TryGetPropertyValue("title", out JsonNode title) && !string.IsNullOrEmpty(title.ToString()))
            {
                Title = title.ToString();
            }
            else if (token.TryGetPropertyValue("navTitle", out JsonNode navTitle) && !string.IsNullOrEmpty(navTitle.ToString()))
            {
                Title = navTitle.ToString();
            }

            if (token.TryGetPropertyValue("description", out JsonNode description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (token.TryGetPropertyValue("keywords", out JsonNode keywords) && !string.IsNullOrEmpty(keywords.ToString()))
            {
                Description = keywords.ToString();
            }
            else if (token.TryGetPropertyValue("catName", out JsonNode catName) && !string.IsNullOrEmpty(catName.ToString()))
            {
                Description = catName.ToString();
            }
            else if (token.TryGetPropertyValue("apkTypeName", out JsonNode apkTypeName) && !string.IsNullOrEmpty(apkTypeName.ToString()))
            {
                Description = apkTypeName.ToString();
            }

            if (token.TryGetPropertyValue("logo", out JsonNode logo) && !string.IsNullOrEmpty(logo.ToString()))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetPropertyValue("lastupdate", out JsonNode lastupdate))
            {
                LastUpdate = lastupdate.ToInt64Safe().ConvertUnixTimeStampToReadable();
            }
        }

        public override string ToString() => Title;
    }
}
