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
            source.LoadMoreStarted -= ProgressBarHelper.ShowProgressBar;
            source.LoadMoreCompleted -= ProgressBarHelper.HideProgressBar;
            source.LoadMoreStarted += ProgressBarHelper.ShowProgressBar;
            source.LoadMoreCompleted += ProgressBarHelper.HideProgressBar;
            tabs.Add(new ShyHeaderItem { Header = header, ItemSource = source });
            return source;
        }

        /// <summary>
        /// 首个 Tab 只依赖帖子 id 时覆盖。详情还在请求时就能开始加载。
        /// </summary>
        protected virtual EntityItemSource CreateEarlyTab() => null;

        /// <summary>取走并清掉提前创建的首个 Tab，避免再 new 一份。</summary>
        protected EntityItemSource TakeEarlyTab()
        {
            EntityItemSource tab = _earlyTab;
            _earlyTab = null;
            return tab;
        }

        private EntityItemSource _earlyTab;

        public override async Task Refresh(bool reset = false)
        {
            if (FeedDetail == null || reset)
            {
                EntityItemSource early = CreateEarlyTab();
                Task earlyTask = Task.CompletedTask;
                if (early != null)
                {
                    early.LoadMoreStarted += ProgressBarHelper.ShowProgressBar;
                    early.LoadMoreCompleted += ProgressBarHelper.HideProgressBar;
                    earlyTask = early.Refresh(true);
                }
                _earlyTab = early;

                FeedDetail = await GetFeedDetailAsync(ID);
                if (FeedDetail == null) { return; }
                Title = FeedDetail.Title;
                List<ShyHeaderItem> tabs = new List<ShyHeaderItem>();
                EntityItemSource firstTab = BuildTabs(tabs);
                // 先绑定列表，回复到了就能上屏，不用等整次 Refresh 返回。
                ItemSource = tabs;
                if (firstTab == null) { return; }
                if (firstTab == early) { await earlyTask; }
                else { await firstTab.Refresh(reset); }
            }
            else
            {
                EntityItemSource source = ItemSource?.FirstOrDefault()?.ItemSource as EntityItemSource;
                if (source != null) { await source.Refresh(reset); }
            }
        }
    }
}
