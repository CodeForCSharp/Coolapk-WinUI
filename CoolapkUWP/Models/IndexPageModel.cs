using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using System.Text.Json.Nodes;
using System;
using System.Collections.Immutable;
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

    internal class IndexPageMessageCardModel : Entity
    {
        public string Title { get; private set; }
        public bool ShowEntities { get; private set; }
        public string Description { get; private set; }
        public ImmutableArray<Entity> Entities { get; private set; } = ImmutableArray<Entity>.Empty;

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
                ImmutableArray<Entity>.Builder buider = ImmutableArray.CreateBuilder<Entity>();
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
                Entities = buider.ToImmutable();
                ShowEntities = true;
            }
            else { ShowEntities = false; }
        }

        public override string ToString() => $"{Title} - {Description}";
    }

    internal enum EntityType
    {
        Image,
        Others,
        TabLink,
        IconLink,
        TextLinks,
        GridLink,
        SelectorLink,
    }

    internal class IndexPageHasEntitiesModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public bool ShowPic { get; private set; }
        public bool ShowTitle { get; private set; }
        public ImageModel Pic { get; private set; }
        public bool ShowEntities { get; private set; }
        public string Description { get; private set; }
        public string EntityTemplate { get; private set; }
        public EntityType EntitiesType { get; private set; }
        public ImmutableArray<Entity> Entities { get; private set; } = ImmutableArray<Entity>.Empty;

        public IndexPageHasEntitiesModel(JsonObject token, EntityType type) : base(token)
        {
            EntitiesType = type;

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            if (token.TryGetPropertyValue("url", out JsonNode url))
            {
                Url = url.ToString();
            }

            if (token.TryGetPropertyValue("description", out JsonNode description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (token.TryGetPropertyValue("release_time", out JsonNode release_time) && !string.IsNullOrEmpty(release_time.ToString()))
            {
                Description = "发布日期：" + release_time.ToString();
            }
            else if (token.TryGetPropertyValue("link_tag", out JsonNode link_tag) && !string.IsNullOrEmpty(link_tag.ToString()))
            {
                Description = link_tag.ToString();
            }
            else if (token.TryGetPropertyValue("hot_num_txt", out JsonNode hot_num_txt) && !string.IsNullOrEmpty(hot_num_txt.ToString()))
            {
                Description = hot_num_txt.ToString() + "热度";
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

            if (token.TryGetPropertyValue("entityTemplate", out JsonNode entityTemplate))
            {
                EntityTemplate = entityTemplate.ToString();
            }

            if (token.TryGetPropertyValue("entities", out JsonNode entities) && entities.AsArray().Count > 0)
            {
                ImmutableArray<Entity>.Builder buider = ImmutableArray.CreateBuilder<Entity>();
                foreach (JsonNode item in entities.AsArray())
                {
                    JsonObject itemObj = item.AsObject();
                    if (itemObj.TryGetPropertyValue("entityType", out JsonNode entityType))
                    {
                        try { itemObj["entityForward"] = EntityTemplate; }
                        catch (Exception ex) { SettingsHelper.LogManager.GetLogger(nameof(IndexPageModel)).Warn(ex.ExceptionToMessage(), ex); }
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

                Entities = buider.ToImmutable();
                ShowEntities = true;
            }
            else { ShowEntities = false; }

            if (token.TryGetPropertyValue("pic", out JsonNode pic) && !string.IsNullOrEmpty(pic.ToString()))
            {
                Pic = new ImageModel(pic.ToString(), ImageType.OriginImage);
                ShowPic = true;
            }
            else { ShowPic = false; }

            ShowTitle = !(string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Url));
        }

        public override string ToString() => $"{Title} - {Description}";
    }

    internal enum OperationType
    {
        Login,
        Refresh,
        ShowTitle,
    }

    internal class IndexPageOperationCardModel : Entity, IHasTitle
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string EntityTemplate { get; private set; }
        public OperationType OperationType { get; private set; }

        public IndexPageOperationCardModel(JsonObject token, OperationType type) : base(token)
        {
            OperationType = type;

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            switch (type)
            {
                case OperationType.ShowTitle when token.TryGetPropertyValue("url", out JsonNode v3) && !string.IsNullOrEmpty(v3.ToString()):
                    Url = v3.ToString();
                    break;

                case OperationType.Refresh:
                    Url = "Refresh";
                    break;

                case OperationType.Login:
                    Url = "Login";
                    break;
            }
        }

        public override string ToString() => Title;
    }
}
