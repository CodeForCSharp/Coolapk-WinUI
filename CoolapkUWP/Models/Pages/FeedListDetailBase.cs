using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models.Pages
{
    [INotifyPropertyChanged]
    public abstract partial class FeedListDetailBase : Entity
    {
        [ObservableProperty]
        public partial bool IsCopyEnabled { get; set; }

        protected FeedListDetailBase() { EntityFixed = true; }

        protected FeedListDetailBase(JsonObject token) : base(token)
        {
            EntityFixed = true;
        }
    }

}
