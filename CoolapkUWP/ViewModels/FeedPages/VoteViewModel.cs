using CoolapkUWP.Controls;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.ViewModels.DataSource;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public class VoteViewModel : FeedDetailViewModelBase
    {
        internal VoteViewModel(string id) : base(id) { }

        protected override EntityItemSource BuildTabs(List<ShyHeaderItem> tabs)
        {
            if (FeedDetail.VoteType == 0)
            {
                EntityItemSource first = null;
                foreach (VoteItem vote in FeedDetail.VoteRows)
                {
                    EntityItemSource source = AddTab(tabs, vote.Title, new VoteItemSource(vote.ID.ToString(), vote.VoteID.ToString()));
                    first ??= source;
                }
                return first;
            }

            EntityItemSource firstTab = AddTab(tabs, "观点", new VoteItemSource(string.Empty, FeedDetail.ID.ToString()));
            if (!string.IsNullOrEmpty(FeedDetail.VoteTag))
            {
                AddTab(tabs, "话题", new TagItemSource(FeedDetail.VoteTag));
            }
            return firstTab;
        }
    }
}
