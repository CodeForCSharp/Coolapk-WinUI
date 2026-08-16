using CoolapkUWP.Data.Dtos;

namespace CoolapkUWP.Models
{
    public class Entity
    {
        public bool EntityFixed { get; set; }
        public int EntityID { get; private set; }
        public string EntityIDText { get; private set; }
        public string EntityType { get; private set; }
        public string EntityForward { get; private set; }

        internal EntityLayout Layout => ResolveLayout(EntityType, EntityForward);

        internal EntityKind Kind => ResolveKind(EntityType);

        protected Entity() { }

        protected Entity(EntityDto dto)
        {
            if (dto != null)
            {
                if (!string.IsNullOrEmpty(dto.EntityId))
                {
                    if (int.TryParse(dto.EntityId, out int id))
                    {
                        EntityID = id;
                    }
                    else
                    {
                        EntityIDText = dto.EntityId;
                    }
                }

                EntityType = dto.EntityType;
                EntityForward = dto.EntityForward;
                EntityFixed = dto.EntityFixed is "1" or "true" or "True";
            }
        }

        private static EntityLayout ResolveLayout(string entityType, string entityForward)
        {
            switch (entityType)
            {
                case "user":
                    return entityForward == "iconScrollCard" ? EntityLayout.Mini : EntityLayout.Default;
                case "collection":
                    return entityForward == "iconMiniGridCard" ? EntityLayout.Mini : EntityLayout.Default;
                case "feed":
                    return entityForward == "imageTextScrollCard" ? EntityLayout.FeedImageText : EntityLayout.Default;
                case "imageSquare":
                case "icon":
                case "iconMiniLink":
                case "recentHistory":
                case "iconMini":
                case "IconLink":
                case "dyh":
                case "apk":
                case "appForum":
                case "picCategory":
                case "product":
                case "entity":
                case "topic":
                    return entityForward switch
                    {
                        "imageSquareScrollCard" => EntityLayout.SquareLink,
                        "apkListCard" or "feedListCard" => EntityLayout.List,
                        _ => EntityLayout.Default,
                    };
                default:
                    return EntityLayout.Default;
            }
        }

        private static EntityKind ResolveKind(string entityType)
        {
            switch (entityType)
            {
                case "iconButton":
                case "link":
                    return EntityKind.Link;
                case "imageText":
                    return EntityKind.ImageText;
                case "textLink":
                    return EntityKind.TextLink;
                case "history":
                    return EntityKind.History;
                case "imageSquare":
                case "icon":
                case "iconMiniLink":
                case "recentHistory":
                case "iconMini":
                case "IconLink":
                case "dyh":
                case "apk":
                case "appForum":
                case "picCategory":
                case "product":
                case "entity":
                case "topic":
                    return EntityKind.Icon;
                default:
                    return EntityKind.Unknown;
            }
        }

        public override string ToString() => $"{EntityType} - {EntityID}";
    }

    public class NullEntity : Entity
    {
        public NullEntity() { }
    }
}
