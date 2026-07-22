using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;

namespace CoolapkUWP.Common
{
    public class ScrollProgressProvider : DependencyObject
    {
        private readonly CompositionPropertySet? propSet;
        private readonly ExpressionAnimation? progressBind;
        private readonly ExpressionAnimation? thresholdBind;
        private double lastOffset;
        private bool readyToScroll;
        private double innerProgress;
        private CancellationTokenSource? delayCancellationTokenSource;
        private CompositionPropertySet? scrollPropertySet;

        public ScrollProgressProvider()
        {
            var compositor = Microsoft.UI.Xaml.Media.CompositionTarget.GetCompositorForCurrentThread();
            if (compositor != null)
            {
                propSet = compositor.CreatePropertySet();
                propSet.InsertScalar("progress", 0f);
                propSet.InsertScalar("threshold", 0f);
                propSet.InsertScalar("delayprogress", -1f);

                progressBind = compositor.CreateExpressionAnimation("clamp(prop.progress, 0f, 1f)");
                progressBind.SetReferenceParameter("prop", propSet);

                thresholdBind = compositor.CreateExpressionAnimation("max(prop.threshold, 0f)");
                thresholdBind.SetReferenceParameter("prop", propSet);
            }
        }

        #region Dependency Properties

        public static readonly DependencyProperty ScrollViewerProperty =
            DependencyProperty.Register("ScrollViewer", typeof(ScrollViewer), typeof(ScrollProgressProvider), new PropertyMetadata(null, (s, a) =>
            {
                if (a.NewValue != a.OldValue)
                {
                    if (s is ScrollProgressProvider sender)
                    {
                        sender.ScrollViewerChanged(a.OldValue as ScrollViewer, a.NewValue as ScrollViewer);
                    }
                }
            }));

        private async void ScrollViewerChanged(ScrollViewer? oldSv, ScrollViewer? newSv)
        {
            if (oldSv != null)
            {
                oldSv.ViewChanged -= ScrollViewer_ViewChanged;
                oldSv.Unloaded -= ScrollViewer_Unloaded;

                propSet?.InsertScalar("progress", (float)innerProgress);
                propSet?.InsertScalar("delayprogress", (float)innerProgress);
            }

            if (newSv != null)
            {
                readyToScroll = true;

                if (newSv.VerticalOffset == 0 && (oldSv == null || (oldSv != null && oldSv.VerticalOffset == 0)))
                {
                    StartScrollProgressAnimation(newSv, false);
                }
                else if (newSv.VerticalOffset > Threshold && lastOffset > Threshold)
                {
                    StartScrollProgressAnimation(newSv, true);
                }
                else if (newSv.VerticalOffset == lastOffset)
                {
                    StartScrollProgressAnimation(newSv, false);
                }
                else if (newSv.VerticalOffset < Threshold || lastOffset < Threshold)
                {
                    await SyncScrollView(newSv);
                    StartScrollProgressAnimation(newSv, true);
                }
                newSv.ViewChanged += ScrollViewer_ViewChanged;
                newSv.Unloaded += ScrollViewer_Unloaded;
            }
        }

        public static readonly DependencyProperty ThresholdProperty =
            DependencyProperty.Register("Threshold", typeof(double), typeof(ScrollProgressProvider), new PropertyMetadata(0d, (s, a) =>
            {
                if (a.NewValue != a.OldValue)
                {
                    if (s is ScrollProgressProvider sender)
                    {
                        double val = (double)a.NewValue;
                        if (val < 0)
                        {
                            throw new ArgumentException($"{nameof(Threshold)}不能小于0");
                        }

                        sender.propSet?.InsertScalar("threshold", (float)val);
                        sender.OnProgressChanged();
                    }
                }
            }));

        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(ScrollProgressProvider), new PropertyMetadata(0d, (s, a) =>
            {
                if (a.NewValue != a.OldValue)
                {
                    if ((double)a.NewValue > 1)
                        throw new ArgumentException($"{nameof(Progress)}不能大于1");
                    if ((double)a.NewValue < 0)
                        throw new ArgumentException($"{nameof(Progress)}不能小于0");

                    if (s is ScrollProgressProvider sender)
                    {
                        if (sender.innerProgress != (double)a.NewValue)
                        {
                            _ = sender.SyncScrollView(sender.ScrollViewer);
                        }
                        sender.OnProgressChanged();
                    }
                }
            }));

        #endregion Dependency Properties

        #region Properties

        public ScrollViewer ScrollViewer
        {
            get => (ScrollViewer)GetValue(ScrollViewerProperty);
            set => SetValue(ScrollViewerProperty, value);
        }

        public double Threshold
        {
            get => (double)GetValue(ThresholdProperty);
            set => SetValue(ThresholdProperty, value);
        }

        public double Progress
        {
            get => (double)GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        #endregion Properties

        #region Methods

        private async void StartScrollProgressAnimation(ScrollViewer sv, bool delay)
        {
            if (propSet == null) return;

            readyToScroll = false;
            if (delayCancellationTokenSource != null)
            {
                delayCancellationTokenSource.Cancel();
                delayCancellationTokenSource = null;
            }

            if (delay)
            {
                delayCancellationTokenSource = new CancellationTokenSource();
            }

            try
            {
                var scrollVisual = ElementCompositionPreview.GetElementVisual(sv);
                scrollPropertySet = scrollVisual?.Properties;

                var compositor = Microsoft.UI.Xaml.Media.CompositionTarget.GetCompositorForCurrentThread();
                if (compositor != null && scrollPropertySet != null)
                {
                    var exp = compositor.CreateExpressionAnimation(
                        $"(prop.delayprogress >= 0) ? prop.delayprogress : clamp(-sv.Translation.Y, 0f, prop.threshold) / prop.threshold");
                    exp.SetReferenceParameter("sv", scrollPropertySet);
                    exp.SetReferenceParameter("prop", propSet);

                    propSet.StartAnimation("progress", exp);

                    if (delay)
                    {
                        await Task.Delay(150, delayCancellationTokenSource.Token);
                    }

                    propSet.InsertScalar("delayprogress", -1f);
                }
            }
            catch
            {
            }
        }

        private async Task SyncScrollView(ScrollViewer sv)
        {
            if (sv == null) return;

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            sv.ViewChanged += Sv_ViewChanged;
            sv.ChangeView(null, Threshold * Progress, null, true);
            await tcs.Task;

            void Sv_ViewChanged(object? sender, ScrollViewerViewChangedEventArgs e)
            {
                if (!e.IsIntermediate)
                {
                    sv.ViewChanged -= Sv_ViewChanged;
                    tcs.SetResult(true);
                }
            }
        }

        public CompositionPropertySet? GetProgressPropertySet()
        {
            if (propSet == null) return null;

            var compositor = Microsoft.UI.Xaml.Media.CompositionTarget.GetCompositorForCurrentThread();
            if (compositor == null) return null;

            CompositionPropertySet _propSet = compositor.CreatePropertySet();
            _propSet.InsertScalar("progress", (float)innerProgress);
            _propSet.InsertScalar("threshold", (float)Threshold);
            _propSet.StartAnimation("progress", progressBind);
            _propSet.StartAnimation("threshold", thresholdBind);
            return _propSet;
        }

        #endregion Methods

        #region Event Callback Methods

        private void ScrollViewer_Unloaded(object sender, RoutedEventArgs e)
        {
            var sv = (ScrollViewer)sender;
            sv.Unloaded -= ScrollViewer_Unloaded;
            sv.ViewChanged -= ScrollViewer_ViewChanged;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var sv = (ScrollViewer)sender;
            lastOffset = sv.VerticalOffset;
            innerProgress = GetProgress(lastOffset, Threshold);
            Progress = innerProgress;

            if (readyToScroll)
            {
                readyToScroll = false;
                StartScrollProgressAnimation(sv, true);
            }
        }

        #endregion Event Callback Methods

        #region Events

        public event TypedEventHandler<object, double>? ProgressChanged;
        protected void OnProgressChanged()
        {
            ProgressChanged?.Invoke(this, Progress);
        }

        #endregion Events

        #region Utilities

        private static double GetProgress(double offset, double threshold)
        {
            return threshold == 0 ? 0 : Math.Min(1, Math.Max(0, offset / threshold));
        }

        #endregion Utilities
    }
}
