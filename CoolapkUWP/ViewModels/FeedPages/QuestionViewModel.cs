using CoolapkUWP.Controls;
using CoolapkUWP.ViewModels.DataSource;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public class QuestionViewModel : FeedDetailViewModelBase
    {
        internal QuestionViewModel(string id) : base(id) { }

        protected override EntityItemSource BuildTabs(List<ShyHeaderItem> tabs)
        {
            EntityItemSource first = AddTab(tabs, "热度排序", new QuestionItemSource(ID, "reply"));
            AddTab(tabs, "点赞排序", new QuestionItemSource(ID, "like"));
            AddTab(tabs, "时间排序", new QuestionItemSource(ID, "dateline"));
            return first;
        }
    }
}
