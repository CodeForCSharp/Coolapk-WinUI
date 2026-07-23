using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    public class TopicModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string FollowNum { get; private set; }
        public string Description { get; private set; }
        public string CommentNum { get; private set; }
        public string LastUpdate { get; private set; }
        public ImageModel Logo { get; private set; }

        public ImageModel Pic => Logo;

        public TopicModel(JsonObject token) : base(token)
        {
            if (token.TryGetPropertyValue("url", out JsonNode url) && !string.IsNullOrEmpty(url.ToString()))
            {
                Url = url.ToString();
            }

            if (token.TryGetPropertyValue("title", out JsonNode title) && !string.IsNullOrEmpty(title.ToString()))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("follownum", out JsonNode follownum) && !string.IsNullOrEmpty(follownum.ToString()))
            {
                FollowNum = follownum.ToString();
            }
            else if (token.TryGetPropertyValue("follow_num", out JsonNode follow_num) && !string.IsNullOrEmpty(follow_num.ToString()))
            {
                FollowNum = follow_num.ToString();
            }

            if (token.TryGetPropertyValue("logo", out JsonNode logo) && !string.IsNullOrEmpty(logo.ToString()))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetPropertyValue("newsnum", out JsonNode newsnum) && !string.IsNullOrEmpty(newsnum.ToString()))
            {
                CommentNum = newsnum.ToString();
            }
            else if (token.TryGetPropertyValue("commentnum", out JsonNode commentnum) && !string.IsNullOrEmpty(commentnum.ToString()))
            {
                CommentNum = commentnum.ToString();
            }
            else if (token.TryGetPropertyValue("rating_total_num", out JsonNode rating_total_num) && !string.IsNullOrEmpty(rating_total_num.ToString()))
            {
                CommentNum = rating_total_num.ToString();
            }

            if (token.TryGetPropertyValue("description", out JsonNode description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (token.TryGetPropertyValue("newtitle", out JsonNode newtitle) && !string.IsNullOrEmpty(newtitle.ToString()))
            {
                Description = newtitle.ToString();
            }
            else if (token.TryGetPropertyValue("username", out JsonNode username) && !string.IsNullOrEmpty(username.ToString()))
            {
                Description = "作者" + username.ToString();
            }
            else if (token.TryGetPropertyValue("rss_type", out JsonNode rss_type) && !string.IsNullOrEmpty(rss_type.ToString()))
            {
                Description = rss_type.ToString();
            }
            else if (token.TryGetPropertyValue("hot_num", out JsonNode hot_num) && !string.IsNullOrEmpty(hot_num.ToString()))
            {
                Description = DataHelper.GetNumString(double.Parse(hot_num.ToString())) + "热度";
            }

            if (token.TryGetPropertyValue("lastupdate", out JsonNode lastupdate) && !string.IsNullOrEmpty(lastupdate.ToString()))
            {
                LastUpdate = lastupdate.ToInt64Safe().ConvertUnixTimeStampToReadable();
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }
}
