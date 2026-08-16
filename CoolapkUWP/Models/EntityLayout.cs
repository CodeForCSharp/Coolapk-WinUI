namespace CoolapkUWP.Models
{
    /// <summary>
    /// 实体渲染变体：由父卡片模板（entityForward）决定，表示"紧凑/特殊排版"。
    /// </summary>
    internal enum EntityLayout
    {
        Default,
        Mini,
        FeedImageText,
        SquareLink,
        List,
    }

    /// <summary>
    /// 实体语义种类：由 wire entityType 决定，供模板选择器分发（替代原始字符串比较）。
    /// </summary>
    internal enum EntityKind
    {
        Unknown = 0,
        Icon,
        Link,
        ImageText,
        TextLink,
        History,
    }
}
