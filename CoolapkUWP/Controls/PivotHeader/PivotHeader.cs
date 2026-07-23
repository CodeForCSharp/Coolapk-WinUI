using CommunityToolkit.WinUI;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace CoolapkUWP.Controls
{
    public sealed class PivotHeader : ListBox
    {
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
        }
    }
}
