using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Models.Users;
using System.Text.Json.Nodes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoolapkUWP.Controls.DataTemplates
{
    public sealed partial class CardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate List { get; set; }
        public DataTemplate Images { get; set; }
        public DataTemplate Others { get; set; }
        public DataTemplate FeedReply { get; set; }
        public DataTemplate IconLinks { get; set; }
        public DataTemplate LoginCard { get; set; }
        public DataTemplate TitleCard { get; set; }
        public DataTemplate CommentMe { get; set; }
        public DataTemplate LikeNotify { get; set; }
        public DataTemplate AtCommentMe { get; set; }
        public DataTemplate RefreshCard { get; set; }
        public DataTemplate MessageCard { get; set; }
        public DataTemplate SubtitleList { get; set; }
        public DataTemplate MessageNotify { get; set; }
        public DataTemplate Rating { get; set; }
        public DataTemplate GridScrollCard { get; set; }
        public DataTemplate ImageTextScrollCard { get; set; }
        public DataTemplate IconLongTitleGridCard { get; set; }
        public DataTemplate IconGridCard { get; set; }
        public DataTemplate IconListCard { get; set; }
        public DataTemplate FeedListCard { get; set; }
        public DataTemplate ListCard { get; set; }
        public DataTemplate ColorfulScrollCard { get; set; }
        public DataTemplate IconMiniGridCard { get; set; }
        public DataTemplate IconMiniScrollCard { get; set; }
        public DataTemplate ImageScaleCard { get; set; }
        public DataTemplate SelectorLinkCard { get; set; }
        public DataTemplate ArticleNewsSingle { get; set; }
        public DataTemplate ArticleNewsMulti { get; set; }
        public DataTemplate ProductTimelineListCard { get; set; }
        public DataTemplate SortSelectCard { get; set; }
        public DataTemplate CapsuleListCard { get; set; }
        public DataTemplate LiveTopic { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is ArticleNewsModel articleNews) { return articleNews.IsMultiPic ? ArticleNewsMulti : ArticleNewsSingle; }
            else if (item is FeedModel feed) { return feed.IsRatingFeed ? Rating : Feed; }
            else if (item is UserModel) { return User; }
            else if (item is FeedReplyModel) { return FeedReply; }
            else if (item is LiveTopicModel) { return LiveTopic; }
            else if (item is IndexPageMessageCardModel) { return MessageCard; }
            else if (item is IconLongTitleGridCardModel) { return IconLongTitleGridCard; }
            else if (item is IconGridCardModel) { return IconGridCard; }
            else if (item is IconListCardModel) { return IconListCard; }
            else if (item is FeedListCardModel) { return FeedListCard; }
            else if (item is ListCardModel) { return ListCard; }
            else if (item is ColorfulScrollCardModel) { return ColorfulScrollCard; }
            else if (item is IconMiniGridCardModel) { return IconMiniGridCard; }
            else if (item is IconMiniScrollCardModel) { return IconMiniScrollCard; }
            else if (item is ImageScaleCardModel) { return ImageScaleCard; }
            else if (item is SelectorLinkCardModel) { return SelectorLinkCard; }
            else if (item is ProductTimelineListCardModel) { return ProductTimelineListCard; }
            else if (item is SortSelectCardModel) { return SortSelectCard; }
            else if (item is CapsuleListCardModel) { return CapsuleListCard; }
            else if (item is IndexPageHasEntitiesModel IndexPageHasEntitiesModel)
            {
                switch (IndexPageHasEntitiesModel.EntitiesType)
                {
                    case EntityType.Image: return Images;
                    case EntityType.IconLink: return IconLinks;
                    case EntityType.GridLink: return GridScrollCard;
                    case EntityType.Others:
                    default: return ImageTextScrollCard;
                }
            }
            else if (item is IndexPageOperationCardModel IndexPageOperationCardModel)
            {
                switch (IndexPageOperationCardModel.OperationType)
                {
                    case OperationType.Refresh: return RefreshCard;
                    case OperationType.Login: return LoginCard;
                    case OperationType.ShowTitle: return TitleCard;
                    default: return Others;
                }
            }
            else if (item is LikeNotificationModel) { return LikeNotify; }
            else if (item is SimpleNotificationModel) { return CommentMe; }
            else if (item is MessageNotificationModel) { return MessageNotify; }
            else if (item is AtCommentMeNotificationModel) { return AtCommentMe; }
            else if (item is IHasDescription) { return List; }
            else if (item is IHasSubtitle) { return SubtitleList; }
            else { return Others; }
        }
    }

    public sealed partial class DetailTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Others { get; set; }
        public DataTemplate DyhDetail { get; set; }
        public DataTemplate UserDetail { get; set; }
        public DataTemplate TopicDetail { get; set; }
        public DataTemplate ProductDetail { get; set; }
        public DataTemplate CollectionDetail { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is DyhDetail) { return DyhDetail; }
            if (item is UserDetail) { return UserDetail; }
            if (item is TopicDetail) { return TopicDetail; }
            if (item is ProductDetail) { return ProductDetail; }
            if (item is CollectionDetail) { return CollectionDetail; }
            return Others;
        }
    }

    public sealed partial class ItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate List { get; set; }
        public DataTemplate Link { get; set; }
        public DataTemplate Empty { get; set; }
        public DataTemplate IconLink { get; set; }
        public DataTemplate MiniUser { get; set; }
        public DataTemplate FeedReply { get; set; }
        public DataTemplate ImageText { get; set; }
        public DataTemplate MiniIconLink { get; set; }
        public DataTemplate SubtitleList { get; set; }
        public DataTemplate FeedImageText { get; set; }
        public DataTemplate SquareLinkCard { get; set; }
        public DataTemplate Rating { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is UserModel user)
            {
                return user.Layout == EntityLayout.Mini ? MiniUser : User;
            }
            else if (item is SourceFeedModel feed)
            {
                if (feed.IsRatingFeed) { return Rating; }
                return feed.Layout == EntityLayout.FeedImageText ? FeedImageText : Feed;
            }
            else if (item is CollectionModel collection)
            {
                return collection.Layout == EntityLayout.Mini ? MiniIconLink : List;
            }
            else if (item is FeedReplyModel) { return FeedReply; }
            else if (item is IndexPageModel indexPage)
            {
                switch (indexPage.Kind)
                {
                    case EntityKind.Icon:
                        return indexPage.Layout switch
                        {
                            EntityLayout.SquareLink => SquareLinkCard,
                            EntityLayout.List => List,
                            _ => IconLink,
                        };
                    case EntityKind.Link: return Link;
                    case EntityKind.ImageText: return ImageText;
                    default: return Empty;
                }
            }
            else
            {
                return item is IHasDescription ? List : item is IHasSubtitle ? SubtitleList : Empty;
            }
        }
    }

    public sealed partial class IconListItemSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate List { get; set; }
        public DataTemplate Icon { get; set; }
        public DataTemplate Rating { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is FeedModel feed)
            {
                return feed.IsRatingFeed ? Rating : Feed;
            }
            else if (item is UserModel) { return User; }
            else if (item is CollectionModel) { return List; }
            return Icon;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed partial class IconGridItemSelector : DataTemplateSelector
    {
        public DataTemplate Icon { get; set; }
        public DataTemplate User { get; set; }
        public DataTemplate List { get; set; }
        public DataTemplate Empty { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is UserModel) { return User; }
            if (item is CollectionModel) { return List; }
            if (item is IndexPageModel) { return Icon; }
            return Empty;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed partial class FeedListItemSelector : DataTemplateSelector
    {
        public DataTemplate Feed { get; set; }
        public DataTemplate Rating { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is FeedModel feed && feed.IsRatingFeed ? Rating : Feed;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed partial class ProfileCardTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Others { get; set; }
        public DataTemplate TitleCard { get; set; }
        public DataTemplate TextLinkList { get; set; }
        public DataTemplate ImageTextScrollCard { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is IndexPageHasEntitiesModel IndexPageHasEntitiesModel)
            {
                switch (IndexPageHasEntitiesModel.EntitiesType)
                {
                    case EntityType.TextLinks: return TextLinkList;
                    default: return ImageTextScrollCard;
                }
            }
            else if (item is IndexPageOperationCardModel IndexPageOperationCardModel)
            {
                switch (IndexPageOperationCardModel.OperationType)
                {
                    case OperationType.ShowTitle: return TitleCard;
                    default: return Others;
                }
            }
            else { return Others; }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed partial class ProfileItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Empty { get; set; }
        public DataTemplate History { get; set; }
        public DataTemplate IconLink { get; set; }
        public DataTemplate TextLink { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            if (item is CollectionModel)
            {
                return History;
            }
            else if (item is IndexPageModel indexPage)
            {
                switch (indexPage.Kind)
                {
                    case EntityKind.Icon: return IconLink;
                    case EntityKind.TextLink: return TextLink;
                    case EntityKind.History: return History;
                    default: return Empty;
                }
            }
            else { return Empty; }
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public sealed partial class SearchTemplateSelector : DataTemplateSelector
    {
        public DataTemplate App { get; set; }
        public DataTemplate SearchWord { get; set; }

        protected override DataTemplate SelectTemplateCore(object item)
        {
            return item is AppModel ? App : SearchWord;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container) => SelectTemplateCore(item);
    }

    public static class EntityTemplateSelector
    {
        public static Entity GetEntity(JsonObject json, bool isHotFeedPage = false)
        {
            if (json.TryGetPropertyValue("entityTemplate", out JsonNode templatePreCheck)
                && templatePreCheck?.ToString() == "articleNews")
            {
                return ArticleNewsModel.FromJson(json);
            }

            switch ((string)json["entityType"])
            {
                case "feed":
                case "discovery": return FeedModel.FromJson(json, isHotFeedPage);
                case "user": return UserModel.FromJson(json);
                case "topic": return TopicModel.FromJson(json);
                case "history": return HistoryModel.FromJson(json);
                case "collection": return CollectionModel.FromJson(json);
                case "product": return ProductModel.FromJson(json);
                case "productBrand": return IndexPageModel.FromJson(json);
                case "liveTopic": return LiveTopicModel.FromJson(json);
                case "entity_type_user_card_manager": return IndexPageOperationCardModel.FromJson(json, OperationType.ShowTitle);
                default:
                    if (json.TryGetPropertyValue("entityTemplate", out JsonNode entityTemplate) && !string.IsNullOrEmpty(entityTemplate.ToString()))
                    {
                        switch (entityTemplate.ToString())
                        {
                            case "feed": return FeedModel.FromJson(json, isHotFeedPage);
                            case "imageSquareScrollCard":
                            case "iconScrollCard":
                            case "feedScrollCard":
                            case "imageTextScrollCard":
                            case "colorfulFatScrollCard":
                            case "linkCard":
                            case "iconButtonGridCard":
                            case "apkScrollCardWithBackground":
                            case "imageScrollCard":
                            case "apkScrollCard":
                            case "gridCard": return IndexPageHasEntitiesModel.FromJson(json, EntityType.Others);
                            case "iconLongTitleGridCard": return IconLongTitleGridCardModel.FromJson(json);
                            case "iconMiniScrollCard": return IconMiniScrollCardModel.FromJson(json);
                            case "imageScaleCard": return ImageScaleCardModel.FromJson(json);
                            case "iconGridCard": return IconGridCardModel.FromJson(json);
                            case "iconListCard": return IconListCardModel.FromJson(json);
                            case "iconMiniLinkGridCard":
                            case "iconMiniGridCard": return IconMiniGridCardModel.FromJson(json);
                            case "productTimelineListCard": return ProductTimelineListCardModel.FromJson(json);
                            case "sortSelectCard": return SortSelectCardModel.FromJson(json);
                            case "capsuleListCard": return CapsuleListCardModel.FromJson(json);
                            case "headCard":
                            case "imageCarouselCard_1":
                            case "imageCard": return IndexPageHasEntitiesModel.FromJson(json, EntityType.Image);
                            case "configCard":
                                return json.TryGetPropertyValue("url", out JsonNode url) && url.ToString().Length >= 5
                                    ? IndexPageHasEntitiesModel.FromJson(json, EntityType.IconLink)
                                    : null;
                            case "iconLinkGridCard": return IndexPageHasEntitiesModel.FromJson(json, EntityType.IconLink);
                            case "feedGroupListCard":
                            case "imageTextGridCard":
                            case "apkListCard":
                            case "textLinkListCard": return IndexPageHasEntitiesModel.FromJson(json, EntityType.TextLinks);
                            case "feedListCard": return FeedListCardModel.FromJson(json);
                            case "listCard": return ListCardModel.FromJson(json);
                            case "colorfulScrollCard": return ColorfulScrollCardModel.FromJson(json);
                            case "textCard":
                            case "messageCard": return IndexPageMessageCardModel.FromJson(json);
                            case "refreshCard": return IndexPageOperationCardModel.FromJson(json, OperationType.Refresh);
                            case "unLoginCard": return IndexPageOperationCardModel.FromJson(json, OperationType.Login);
                            case "titleCard": return IndexPageOperationCardModel.FromJson(json, OperationType.ShowTitle);
                            case "iconTabLinkGridCard": return IndexPageHasEntitiesModel.FromJson(json, EntityType.TabLink);
                            case "selectorLinkCard": return SelectorLinkCardModel.FromJson(json);
                            default: return null;
                        }
                    }
                    return null;
            }
        }
    }
}
