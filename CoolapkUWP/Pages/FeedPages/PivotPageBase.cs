using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 以 Pivot 为主体的标签页基类：统一"索引记忆 + 惰性导航 + 刷新委托"流程。
    /// </summary>
    public abstract class PivotPageBase : Page, INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<Type, int> pivotIndices = new ConcurrentDictionary<Type, int>();

        /// <summary>按页面类型记忆的选中索引（导航离开后恢复）。</summary>
        protected int PivotIndex
        {
            get => pivotIndices.GetOrAdd(GetType(), 0);
            set => pivotIndices[GetType()] = value;
        }

        private bool isLoaded;
        protected Func<bool, Task> refresh;

        protected abstract Pivot PivotControl { get; }

        /// <summary>返回各标签页；返回 null 表示标签页已在 XAML 中声明。</summary>
        protected abstract ObservableCollection<PivotItem> GetMainItems();

        /// <summary>选中标签页且其 Frame 内容为空时，导航到目标页。</summary>
        protected virtual void NavigateToPage(PivotItem item, Frame frame) { }

        /// <summary>选中标签页但其内容不是 Frame 时，按需建立刷新委托。</summary>
        protected virtual void OnTabSelected(PivotItem item) { }

        /// <summary>Pivot 每次加载完成后调用。</summary>
        protected virtual void OnPivotLoaded() { }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PivotIndex = PivotControl.SelectedIndex;
        }

        protected void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isLoaded)
            {
                ObservableCollection<PivotItem> items = GetMainItems();
                if (items != null) { PivotControl.ItemsSource = items; }
                PivotControl.SelectedIndex = PivotIndex;
                isLoaded = true;
            }
            OnPivotLoaded();
            UpdateCurrentTab();
        }

        protected void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateCurrentTab();

        private void UpdateCurrentTab()
        {
            PivotItem item = PivotControl.SelectedItem as PivotItem;
            if (item == null) { return; }
            if (item.Content is Frame frame && frame.Content is null)
            {
                NavigateToPage(item, frame);
                refresh = (reset) => (frame.Content as AdaptivePage)?.Refresh(reset);
            }
            else if (item.Content is Frame frame2 && frame2.Content is AdaptivePage adaptivePage)
            {
                refresh = (reset) => adaptivePage.Refresh(reset);
            }
            else
            {
                OnTabSelected(item);
            }
        }

        protected virtual void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (refresh != null) { _ = refresh(true); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }
    }
}
