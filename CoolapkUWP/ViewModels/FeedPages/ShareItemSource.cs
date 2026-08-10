using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Users;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public partial class ShareItemSource : EntityItemSource
    {
        public string ID;

        public ShareItemSource(string id, string feedtype = "feed")
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, _, __) =>
                UriHelper.GetUri(
                    UriType.GetShareList,
                    id,
                    feedtype,
                    p),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JsonObject json)
        {
            yield return new FeedModel(json);
        }
    }

}
