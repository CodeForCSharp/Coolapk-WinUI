namespace CoolapkUWP.Models
{
    /// <summary>
    /// 首页卡片描述的候选字段回退解析：按固定优先级取第一个非空字段。
    /// </summary>
    internal static class DescriptionResolver
    {
        public static string Resolve(
            string description,
            string releaseTime,
            string linkTag,
            string hotNumTxt,
            string keywords,
            string catName,
            string apkTypeName,
            string typeName,
            string rssType,
            string subTitle,
            string releaseTimeLabel,
            string hotNumLabel)
        {
            if (!string.IsNullOrEmpty(description)) { return description; }
            if (!string.IsNullOrEmpty(releaseTime)) { return $"{releaseTimeLabel}{releaseTime}"; }
            if (!string.IsNullOrEmpty(linkTag)) { return linkTag; }
            if (!string.IsNullOrEmpty(hotNumTxt)) { return $"{hotNumTxt}{hotNumLabel}"; }
            if (!string.IsNullOrEmpty(keywords)) { return keywords; }
            if (!string.IsNullOrEmpty(catName)) { return catName; }
            if (!string.IsNullOrEmpty(apkTypeName)) { return apkTypeName; }
            if (!string.IsNullOrEmpty(typeName)) { return typeName; }
            if (!string.IsNullOrEmpty(rssType)) { return rssType; }
            return subTitle ?? string.Empty;
        }
    }
}
