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
    public partial class QuestionItemSource : EntityItemSource
    {
        public string ID;

        public QuestionItemSource(string id, string answerSortType = "reply")
        {
            ID = id;
            Provider = new CoolapkListProvider(
                (p, firstItem, lastItem) =>
                UriHelper.GetUri(
                    UriType.GetAnswers,
                    id,
                    answerSortType,
                    p,
                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                GetEntities,
                "id");
        }

        private IEnumerable<Entity> GetEntities(JsonObject json)
        {
            yield return new FeedModel(json);
        }
    }

}
