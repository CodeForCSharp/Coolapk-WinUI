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
    public partial class LikeItemSource : EntityItemSource
    {
        public string ID;

        public LikeItemSource(string id)
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetLikeList,
                    id,
                    p,
                    p > 1 ? $"&firstItem={firstItem}&lastItem={lastItem}" : string.Empty),
                GetEntities,
                "uid");
        }

        private IEnumerable<Entity> GetEntities(JsonObject json)
        {
            yield return new UserModel(json);
        }
    }

}
