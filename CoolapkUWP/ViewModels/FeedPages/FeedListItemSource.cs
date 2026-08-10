using CoolapkUWP.Controls;
using CoolapkUWP.Controls.DataTemplates;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Dispatching;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public partial class FeedListItemSource : EntityItemSource
    {
        public string ID;

        public FeedListItemSource(string id, CoolapkListProvider provider)
        {
            ID = id;
            Provider = provider;
        }

        protected override async Task AddItemsAsync(IList<Entity> items)
        {
            if (items == null) { return; }
            await Dispatcher.ResumeForegroundAsync();
            foreach (Entity item in items)
            {
                if (item is NullEntity) { continue; }
                Add(item);
            }
        }
    }
}
