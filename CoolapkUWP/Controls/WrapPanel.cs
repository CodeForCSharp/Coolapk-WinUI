using System;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    /// <summary>
    /// 按内容宽度横向排列，放不下就换行。每个子元素保持自己的宽度，不会被截成同一尺寸。
    /// </summary>
    public partial class WrapPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            double limit = availableSize.Width;
            bool wrap = !double.IsInfinity(limit) && limit > 0;
            double x = 0;
            double y = 0;
            double lineHeight = 0;
            double maxWidth = 0;

            foreach (UIElement child in Children)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double childWidth = child.DesiredSize.Width;
                double childHeight = child.DesiredSize.Height;
                if (wrap && x > 0 && x + childWidth > limit)
                {
                    maxWidth = Math.Max(maxWidth, x);
                    x = 0;
                    y += lineHeight;
                    lineHeight = 0;
                }

                x += childWidth;
                lineHeight = Math.Max(lineHeight, childHeight);
            }

            maxWidth = Math.Max(maxWidth, x);
            return new Size(wrap ? limit : maxWidth, y + lineHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double x = 0;
            double y = 0;
            double lineHeight = 0;

            foreach (UIElement child in Children)
            {
                double childWidth = child.DesiredSize.Width;
                double childHeight = child.DesiredSize.Height;
                if (x > 0 && x + childWidth > finalSize.Width)
                {
                    x = 0;
                    y += lineHeight;
                    lineHeight = 0;
                }

                child.Arrange(new Rect(x, y, childWidth, childHeight));
                x += childWidth;
                lineHeight = Math.Max(lineHeight, childHeight);
            }

            return finalSize;
        }
    }
}
