using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CirclePage : Page
    {
        private static int PivotIndex = 0;

        private bool isLoaded;
        private Func<bool, Task> Refresh;

        public CirclePage() => InitializeComponent();

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PivotIndex = Pivot.SelectedIndex;
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isLoaded)
            {
                var items = GetMainItems();
                Pivot.TabItems.Clear();
                foreach (var it in items) Pivot.TabItems.Add(it);
                Pivot.SelectedIndex = PivotIndex;
                isLoaded = true;
            }
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabViewItem MenuItem = Pivot.SelectedItem as TabViewItem;
            if ((Pivot.SelectedItem as TabViewItem).Content is Frame Frame && Frame.Content is null)
            {
                _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(MenuItem.Tag.ToString().Contains("V") ? $"/page?url={MenuItem.Tag}" : $"/page?url=V9_HOME_TAB_FOLLOW&type={MenuItem.Tag}"));
                Refresh = (reset) => _ = (Frame.Content as AdaptivePage).Refresh(reset);
            }
            else if ((Pivot.SelectedItem as TabViewItem).Content is Frame __ && __.Content is AdaptivePage AdaptivePage)
            {
                Refresh = (reset) => _ = AdaptivePage.Refresh(reset);
            }
        }

        public static ObservableCollection<TabViewItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("CirclePage");
            ObservableCollection<TabViewItem> items = new ObservableCollection<TabViewItem>
            {
                new TabViewItem() { Tag = "V9_HOME_TAB_FOLLOW", Header = loader.GetString("V9_HOME_TAB_FOLLOW"), Content = new Frame() },
                new TabViewItem() { Tag = "circle", Header = loader.GetString("circle"), Content = new Frame() },
                new TabViewItem() { Tag = "apk", Header = loader.GetString("apk"), Content = new Frame() },
                new TabViewItem() { Tag = "topic", Header = loader.GetString("topic"), Content = new Frame() },
                new TabViewItem() { Tag = "question", Header = loader.GetString("question"), Content = new Frame() },
                new TabViewItem() { Tag = "product", Header = loader.GetString("product"), Content = new Frame() }
            };
            return items;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => _ = Refresh(true);
    }
}
