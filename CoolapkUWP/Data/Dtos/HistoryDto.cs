namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 历史记录条目(history)。
    /// </summary>
    public class HistoryDto : EntityDto
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string TargetTypeTitle { get; set; }
        public long? Dateline { get; set; }
        public string Logo { get; set; }
    }
}
