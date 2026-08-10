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
    internal class IndexPageMessageCardModel : Entity
    {
        public string Title { get; private set; }
        public bool ShowEntities { get; private set; }
        public string Description { get; private set; }
        public List<Entity> Entities { get; private set; } = new List<Entity>();

        public IndexPageMessageCardModel(JsonObject token) : base(token)
        {
            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("description", out JsonNode description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (token.TryGetPropertyValue("release_time", out JsonNode release_time) && !string.IsNullOrEmpty(release_time.ToString()))
            {
                Description = $"发布日期：{release_time}";
            }
            else if (token.TryGetPropertyValue("link_tag", out JsonNode link_tag) && !string.IsNullOrEmpty(link_tag.ToString()))
            {
                Description = link_tag.ToString();
            }
            else if (token.TryGetPropertyValue("hot_num_txt", out JsonNode hot_num_txt) && !string.IsNullOrEmpty(hot_num_txt.ToString()))
            {
                Description = $"{hot_num_txt}热度";
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
            else if (token.TryGetPropertyValue("rss_type", out JsonNode rss_type) && !string.IsNullOrEmpty(rss_type.ToString()))
            {
                Description = rss_type.ToString();
            }
            else if (token.TryGetPropertyValue("subTitle", out JsonNode subTitle))
            {
                Description = subTitle.ToString();
            }

            if (token.TryGetPropertyValue("entities", out JsonNode entities) && entities.AsArray().Count > 0)
            {
                List<Entity> buider = new List<Entity>();
                foreach (JsonNode item in entities.AsArray())
                {
                    JsonObject itemObj = item.AsObject();
                    if (itemObj.TryGetPropertyValue("entityType", out JsonNode entityType))
                    {
                        switch (entityType.ToString())
                        {
                            case "feed":
                                buider.Add(new FeedModel(itemObj));
                                break;

                            case "user":
                                buider.Add(new UserModel(itemObj));
                                break;

                            case "collection":
                                buider.Add(new CollectionModel(itemObj));
                                break;

                            default:
                                buider.Add(new IndexPageModel(itemObj));
                                break;
                        }
                    }
                }
                Entities = buider;
                ShowEntities = true;
            }
            else { ShowEntities = false; }
        }

        public override string ToString() => $"{Title} - {Description}";
    }

}
