using CommunityToolkit.WinUI;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;

namespace CoolapkUWP.Controls
{
    public sealed class PivotHeader : ListBox
    {
        private CancellationTokenSource cts;

        public static readonly DependencyProperty PivotProperty =
            DependencyProperty.Register(
                nameof(Pivot),
                typeof(TabView),
                typeof(PivotHeader),
                new PropertyMetadata(null, OnPivotPropertyChanged));

        public TabView Pivot
        {
            get => (TabView)GetValue(PivotProperty);
            set => SetValue(PivotProperty, value);
        }

        private static void OnPivotPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((PivotHeader)d).SetPivot();
            }
        }

        public PivotHeader()
        {
            DefaultStyleKey = typeof(PivotHeader);
            SelectionChanged += ShyHeader_SelectionChanged;
        }

        private void SetPivot()
        {
            if (Pivot == null) { return; }
            SetBinding(SelectedIndexProperty, new Binding
            {
                Source = Pivot,
                Mode = BindingMode.TwoWay,
                Path = new PropertyPath(nameof(SelectedIndex))
            });
            ItemsSource = Pivot.TabItems.Select(item => (item as TabViewItem)?.Header ?? string.Empty).ToArray();
            HidePivotHeader();
        }

        private async void HidePivotHeader()
        {
            cts?.Cancel();
            cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            // WinUI 3: Pivot internal types (PivotPanel) not accessible, skip header hiding
        }

        private void ShyHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionMode != SelectionMode.Single) return;
            // WinUI 3: composition animation needs review
            // The old indicator animation code used ElementCompositionPreview APIs
        }
    }
}
