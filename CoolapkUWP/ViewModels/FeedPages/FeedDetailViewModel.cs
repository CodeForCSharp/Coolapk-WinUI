using CoolapkUWP.Controls;
using CoolapkUWP.ViewModels.DataSource;
using System.Collections.Generic;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public class FeedDetailViewModel : FeedDetailViewModelBase
    {
        internal FeedDetailViewModel(string id) : base(id) { }

        protected override EntityItemSource CreateEarlyTab() => new ReplyItemSource(ID);

        protected override EntityItemSource BuildTabs(List<ShyHeaderItem> tabs)
        {
            return AddTab(tabs, "回复", TakeEarlyTab() ?? new ReplyItemSource(ID));
        }
    }
}
