using CoolapkUWP.Controls;
using CoolapkUWP.ViewModels.DataSource;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public class FeedDetailViewModel : FeedDetailViewModelBase
    {
        internal FeedDetailViewModel(string id) : base(id) { }

        protected override EntityItemSource BuildTabs(List<ShyHeaderItem> tabs)
        {
            EntityItemSource first = AddTab(tabs, "回复", new ReplyItemSource(ID));
            AddTab(tabs, "点赞", new LikeItemSource(ID));
            AddTab(tabs, "转发", new ShareItemSource(ID, FeedDetail.FeedType));
            return first;
        }
    }
}
