namespace CoolapkUWP.Models
{
    public class Entity
    {
        public bool EntityFixed { get; set; }
        public int EntityID { get; private set; }
        public string EntityIDText { get; private set; }
        public string EntityType { get; private set; }
        public string EntityForward { get; private set; }

        protected Entity() { }

        protected void InitializeEntity(string entityId, string entityType, string entityForward, string entityFixed)
        {
            if (!string.IsNullOrEmpty(entityId))
            {
                if (int.TryParse(entityId, out int id))
                {
                    EntityID = id;
                }
                else
                {
                    EntityIDText = entityId;
                }
            }

            EntityType = entityType;
            EntityForward = entityForward;
            EntityFixed = entityFixed is "1" or "true" or "True";
        }

        public override string ToString() => $"{EntityType} - {EntityID}";
    }

    public class NullEntity : Entity
    {
        public NullEntity() { }
    }
}
