namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 所有 DTO 的公共基类,承载实体标识字段。
    /// </summary>
    public abstract class EntityDto
    {
        public string EntityId { get; set; }
        public string EntityType { get; set; }
        public string EntityForward { get; set; }
        public string EntityFixed { get; set; }
    }
}
