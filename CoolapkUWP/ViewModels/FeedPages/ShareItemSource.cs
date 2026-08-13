using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json.Nodes;
using System.Collections.Generic;
using System.Linq;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public partial class ShareItemSource : EntityItemSource
    {
        public ShareItemSource(string id, string feedtype = "feed") : base(id, new CoolapkListProvider(
            (p, _, __) =>
                UriHelper.GetUri(
                    UriType.GetShareList,
                    id,
                    feedtype,
                    p),
            a => DtoJson.DeserializeList<FeedDto>(a).Select(d => new FeedModel(d)),
            "id")) { }
    }
}
