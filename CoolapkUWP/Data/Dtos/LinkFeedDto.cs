using System.Collections.Generic;

namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 链接卡片预览数据(由 LinkPreviewService 从酷安/哔哩哔哩/IT之家响应提取)。
    /// </summary>
    public class LinkFeedDto
    {
        public bool Succeed { get; set; }
        public string Url { get; set; }
        public string Message { get; set; }
        public string MessageTitle { get; set; }
        public long? Dateline { get; set; }
        public List<string> PicUris { get; set; }
        public string UserName { get; set; }
        public string UserUrl { get; set; }
    }
}
