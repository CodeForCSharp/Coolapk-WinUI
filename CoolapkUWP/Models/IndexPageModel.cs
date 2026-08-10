using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models
{
    internal class IndexPageModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string SubTitle { get; private set; }
        public string Description { get; private set; }
        public string EntityTemplate { get; private set; }
        public ImageModel Pic { get; private set; }

        public IndexPageModel(JsonObject token) : base(token)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");

            if (token.TryGetPropertyValue("entityTemplate", out JsonNode entityTemplate))
            {
                EntityTemplate = entityTemplate.ToString();
            }

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("subTitle", out JsonNode subTitle) && !string.IsNullOrEmpty(subTitle.ToString()))
            {
                SubTitle = subTitle.ToString();
            }
            else if (token.TryGetPropertyValue("subtitle", out JsonNode subtitle) && !string.IsNullOrEmpty(subtitle.ToString()))
            {
                SubTitle = subtitle.ToString();
            }
            else if (token.TryGetPropertyValue("hot_num_txt", out JsonNode hot_num_txt) && !string.IsNullOrEmpty(hot_num_txt.ToString()))
            {
                SubTitle = $"{hot_num_txt}{loader.GetString("HotNum")}";
            }
            else if (token.TryGetPropertyValue("link_tag", out JsonNode link_tag) && !string.IsNullOrEmpty(link_tag.ToString()))
            {
                SubTitle = link_tag.ToString();
            }
            else if (token.TryGetPropertyValue("apkTypeName", out JsonNode apkTypeName) && !string.IsNullOrEmpty(apkTypeName.ToString()))
            {
                SubTitle = apkTypeName.ToString();
            }
            else if (token.TryGetPropertyValue("typeName", out JsonNode typeName) && !string.IsNullOrEmpty(typeName.ToString()))
            {
                SubTitle = typeName.ToString();
            }
            else if (token.TryGetPropertyValue("keywords", out JsonNode keywords) && !string.IsNullOrEmpty(keywords.ToString()))
            {
                SubTitle = keywords.ToString();
            }
            else if (token.TryGetPropertyValue("catName", out JsonNode catName) && !string.IsNullOrEmpty(catName.ToString()))
            {
                SubTitle = catName.ToString();
            }
            else if (token.TryGetPropertyValue("rss_type", out JsonNode rss_type) && !string.IsNullOrEmpty(rss_type.ToString()))
            {
                SubTitle = rss_type.ToString();
            }
            else if (token.TryGetPropertyValue("product_num", out JsonNode product_num) && !string.IsNullOrEmpty(product_num.ToString()))
            {
                SubTitle = $"{product_num}{loader.GetString("ProductNum")}";
            }
            else if (token.TryGetPropertyValue("description", out JsonNode description))
            {
                SubTitle = description.ToString();
            }

            if (token.TryGetPropertyValue("video_playback_url", out JsonNode video_playback_url) && !string.IsNullOrEmpty(video_playback_url.ToString()))
            {
                Url = video_playback_url.ToString();
            }
            else if (token.TryGetPropertyValue("url", out JsonNode url))
            {
                Url = url.ToString();
            }

            if (token.TryGetPropertyValue("description", out JsonNode v1) && !string.IsNullOrEmpty(v1.ToString()))
            {
                Description = v1.ToString();
            }
            else if (token.TryGetPropertyValue("release_time", out JsonNode release_time) && !string.IsNullOrEmpty(release_time.ToString()))
            {
                Description = $"{loader.GetString("ReleaseTime")}{release_time}";
            }
            else if (token.TryGetPropertyValue("link_tag", out JsonNode link_tag) && !string.IsNullOrEmpty(link_tag.ToString()))
            {
                Description = link_tag.ToString();
            }
            else if (token.TryGetPropertyValue("hot_num_txt", out JsonNode hot_num_txt) && !string.IsNullOrEmpty(hot_num_txt.ToString()))
            {
                Description = $"{hot_num_txt}{loader.GetString("HotNum")}";
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
            else if (token.TryGetPropertyValue("typeName", out JsonNode typeName) && !string.IsNullOrEmpty(typeName.ToString()))
            {
                Description = typeName.ToString();
            }
            else if (token.TryGetPropertyValue("rss_type", out JsonNode rss_type) && !string.IsNullOrEmpty(rss_type.ToString()))
            {
                Description = rss_type.ToString();
            }
            else if (token.TryGetPropertyValue("subTitle", out JsonNode v2))
            {
                Description = v2.ToString();
            }

            if (token.TryGetPropertyValue("cover_pic", out JsonNode cover_pic) && !string.IsNullOrEmpty(cover_pic.ToString()))
            {
                Pic = new ImageModel(cover_pic.ToString(), ImageType.OriginImage);
            }
            else if (token.TryGetPropertyValue("pic", out JsonNode pic) && !string.IsNullOrEmpty(pic.ToString()))
            {
                Pic = new ImageModel(pic.ToString(), ImageType.OriginImage);
            }
            else if (token.TryGetPropertyValue("logo", out JsonNode logo) && !string.IsNullOrEmpty(logo.ToString()))
            {
                Pic = new ImageModel(logo.ToString(), ImageType.Icon);
            }
            else if (token.TryGetPropertyValue("pic_url", out JsonNode pic_url))
            {
                Pic = new ImageModel(pic_url.ToString(), ImageType.Icon);
            }
        }

        public override string ToString() => $"{Title} - {Description}";
    }

}
