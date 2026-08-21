using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CoolapkUWP.Models.Feeds
{
    internal partial class ArticleNewsModel : FeedModel
    {
        public string NewsTitle { get; private set; }

        public bool IsSinglePic => PicArr.Count == 1;
        public bool IsMultiPic => PicArr.Count > 1;

        /// <summary>多图角标(如 "6图")，不超过 3 图时不显示。</summary>
        public string PicCountText => PicArr.Count > 3 ? $"{PicArr.Count}图" : null;

        /// <summary>信息行(作者 评论数 时间)，如 "竹本青 7评论 54分钟前"。</summary>
        public string MetaText
        {
            get
            {
                List<string> parts = new List<string>();
                if (!string.IsNullOrEmpty(UserInfo?.UserName)) { parts.Add(UserInfo.UserName); }
                if (ReplyNum > 0) { parts.Add($"{ReplyNum}评论"); }
                if (!string.IsNullOrEmpty(Dateline)) { parts.Add(Dateline); }
                return string.Join("  ", parts);
            }
        }

        public ArticleNewsModel(FeedDto dto) : base(dto)
        {
            NewsTitle = ExtractTitle(Message);
        }

        public static new ArticleNewsModel FromJson(JsonObject json)
            => new ArticleNewsModel(DtoJson.Deserialize<FeedDto>(json));

        private static string ExtractTitle(string message)
        {
            if (string.IsNullOrEmpty(message)) { return string.Empty; }

            string firstLine = message.Split('\n')[0];
            firstLine = Regex.Replace(firstLine, "<[^>]+>", string.Empty).Trim();
            if (firstLine.StartsWith("【") && firstLine.EndsWith("】"))
            {
                firstLine = firstLine[1..^1].Trim();
            }
            return firstLine.Length > 100 ? firstLine[..100] : firstLine;
        }
    }
}
