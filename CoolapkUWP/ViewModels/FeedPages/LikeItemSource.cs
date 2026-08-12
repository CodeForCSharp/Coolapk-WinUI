using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json.Nodes;
using System.Collections.Generic;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public partial class LikeItemSource : EntityItemSource
    {
        public LikeItemSource(string id) : base(id, new CoolapkListProvider(
            (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetLikeList,
                    id,
                    p,
                    UriHelper.GetPagingArgs(p, firstItem, lastItem)),
            o => new[] { UserModel.FromJson(o) },
            "uid")) { }
    }
}
