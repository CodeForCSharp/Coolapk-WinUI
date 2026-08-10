using CoolapkUWP.Helpers;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System;

namespace CoolapkUWP.Models
{
    public class Entity
    {
        public bool EntityFixed { get; set; }
        public int EntityID { get; private set; }
        public string EntityIDText { get; private set; }
        public string EntityType { get; private set; }
        public string EntityForward { get; private set; }

        public Entity(JsonObject token)
        {
            if (token == null) { return; }

            if (token.TryGetPropertyValue("entityId", out JsonNode entityId))
            {
                try
                {
                    EntityID = entityId.ToInt32Safe();
                }
                catch (Exception ex)
                {
                    SettingsHelper.LogManager.CreateLogger(nameof(Entity)).LogWarning(ex, ex.ExceptionToMessage());
                    EntityIDText = entityId.ToString();
                }
            }

            if (token.TryGetPropertyValue("entityType", out JsonNode entityType))
            {
                EntityType = entityType.ToString();
            }

            if (token.TryGetPropertyValue("entityFixed", out JsonNode entityFixed))
            {
                EntityFixed = Convert.ToBoolean(entityFixed.ToInt32Safe());
            }

            if (token.TryGetPropertyValue("entityForward", out JsonNode entityForward))
            {
                EntityForward = entityForward.ToString();
            }
        }

        public override string ToString() => $"{EntityType} - {EntityID}";
    }

    public class NullEntity : Entity
    {
        public NullEntity(JsonObject token = null) : base(token) { }
    }
}
