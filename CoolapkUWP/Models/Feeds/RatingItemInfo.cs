namespace CoolapkUWP.Models.Feeds
{
    /// <summary>
    /// 信息流里点评正文的一段：绿色标签 + 一行摘要。
    /// </summary>
    public class RatingSection
    {
        public string Title { get; }
        public string Body { get; }

        public RatingSection(string title, string body)
        {
            Title = title ?? string.Empty;
            Body = body ?? string.Empty;
        }
    }

    /// <summary>
    /// rating feed 的子项评分(续航/影像/性能 等)。
    /// </summary>
    public class RatingItemInfo
    {
        public string Name { get; }
        public int Score { get; }            // 0-10
        public string Descriptor { get; }    // 当前分数对应的 star_desc(如 "非常好")
        public int Star { get; }            // 已填充的星数 0-5

        /// <summary>信息流里跟在维度名后面的星数，如 "5☆"。</summary>
        public string StarText => $"{Star}\u2606";

        /// <summary>五颗星的点亮状态，详情页按行展示。</summary>
        public System.Collections.Generic.List<bool> Stars { get; }

        public RatingItemInfo(string name, int score, System.Collections.Generic.IList<string> starDesc)
        {
            Name = name;
            Score = score;
            int stars = System.Math.Max(0, System.Math.Min(5, (int)System.Math.Round(score / 2.0)));
            Star = stars;
            Stars = new System.Collections.Generic.List<bool>(5);
            for (int i = 0; i < 5; i++) { Stars.Add(i < stars); }
            Descriptor = (starDesc != null && stars >= 1 && stars <= starDesc.Count)
                ? starDesc[stars - 1]
                : string.Empty;
        }
    }
}