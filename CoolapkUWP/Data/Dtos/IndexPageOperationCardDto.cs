namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 首页操作卡片(登录/刷新/标题)。
    /// </summary>
    public class IndexPageOperationCardDto : EntityDto
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }
}
