using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Pages
{
    public abstract partial class FeedListDetailBase : Entity, INotifyPropertyChanged
    {
        [ObservableProperty]
        private bool isCopyEnabled;

        protected FeedListDetailBase(JsonObject token) : base(token)
        {
            EntityFixed = true;
        }
    }

}
