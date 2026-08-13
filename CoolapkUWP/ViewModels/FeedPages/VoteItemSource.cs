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
    public partial class VoteItemSource : EntityItemSource
    {
        public VoteItemSource(string id, string fid) : base(id, new CoolapkListProvider(
            (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetVoteComments,
                    fid,
                    UriHelper.GetOptionalArg("extra_key", id),
                    p,
                    UriHelper.GetOptionalArg("firstItem", firstItem),
                    UriHelper.GetOptionalArg("lastItem", lastItem)),
            a => DtoJson.DeserializeList<FeedDto>(a).Select(d => new FeedModel(d)),
            "id")) { }
    }
}
