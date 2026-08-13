using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.DataSource;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    /// <summary>
    /// 动态/问答/投票详情页的共同基类：统一"构建 Tab 列表 + 刷新首个 Tab"流程。
    /// </summary>
    public abstract partial class FeedDetailViewModelBase : FeedShellViewModel
    {
        protected FeedDetailViewModelBase(string id) : base(id) { }

        /// <summary>
        /// 构建各 Tab，返回首个 Tab 的数据源（用于初始刷新）。
        /// </summary>
        protected abstract EntityItemSource BuildTabs(List<ShyHeaderItem> tabs);

        protected EntityItemSource AddTab(List<ShyHeaderItem> tabs, string header, EntityItemSource source)
        {
            source.LoadMoreStarted += ProgressBarHelper.ShowProgressBar;
            source.LoadMoreCompleted += ProgressBarHelper.HideProgressBar;
            tabs.Add(new ShyHeaderItem { Header = header, ItemSource = source });
            return source;
        }

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                FeedDetail = await GetFeedDetailAsync(ID);
                if (FeedDetail == null) { return; }
                Title = FeedDetail.Title;
                List<ShyHeaderItem> tabs = new List<ShyHeaderItem>();
                EntityItemSource firstTab = BuildTabs(tabs);
                base.ItemSource = tabs;
                if (firstTab != null) { await firstTab.Refresh(reset); }
            }
            else
            {
                EntityItemSource source = ItemSource?.FirstOrDefault()?.ItemSource as EntityItemSource;
                if (source != null) { await source.Refresh(reset); }
            }
        }
    }
}
