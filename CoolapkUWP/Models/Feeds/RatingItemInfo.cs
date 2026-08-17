namespace CoolapkUWP.Models.Feeds
{
    /// <summary>
    /// rating feed 的子项评分(续航/影像/性能 等)。
    /// </summary>
    public class RatingItemInfo
    {
        public string Name { get; }
        public int Score { get; }            // 0-10
        public string Descriptor { get; }    // 当前分数对应的 star_desc(如 "非常好")
        public int Star { get; }            // 已填充的星数 0-5

        public RatingItemInfo(string name, int score, System.Collections.Generic.IList<string> starDesc)
        {
            Name = name;
            Score = score;
            int stars = System.Math.Max(0, System.Math.Min(5, (int)System.Math.Round(score / 2.0)));
            Star = stars;
            Descriptor = (starDesc != null && stars >= 1 && stars <= starDesc.Count)
                ? starDesc[stars - 1]
                : string.Empty;
        }
    }
}