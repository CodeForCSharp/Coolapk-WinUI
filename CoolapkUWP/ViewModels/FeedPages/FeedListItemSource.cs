using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;

namespace CoolapkUWP.ViewModels.FeedPages
{
    /// <summary>
    /// 通用列表型数据源：不展开首页卡片子提供器。
    /// </summary>
    public partial class FeedListItemSource : EntityItemSource
    {
        public FeedListItemSource(string id, CoolapkListProvider provider)
            : base(id, provider, useSubProvider: false) { }
    }
}
