using CoolapkUWP.Common;
using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Feeds
{
    public enum FeedRelationKind
    {
        Product,
        Topic
    }

    /// <summary>
    /// 动态详情页底部关联卡：数码商品或话题。
    /// </summary>
    public partial class FeedRelationCard : ObservableObject
    {
        private int _detailLoadState;

        public FeedRelationKind Kind { get; }

        public int ID { get; }

        public string Url { get; }

        public string Title { get; }

        public string KindLabel { get; }

        public ImageModel Logo { get; }

        public bool IsProduct => Kind == FeedRelationKind.Product;

        public bool IsTopic => Kind == FeedRelationKind.Topic;

        [ObservableProperty]
        public partial string Score { get; set; }

        [ObservableProperty]
        public partial double ScoreValue { get; set; }

        [ObservableProperty]
        public partial string HotNum { get; set; }

        [ObservableProperty]
        public partial string CommentNum { get; set; }

        [ObservableProperty]
        public partial bool Followed { get; set; }

        [ObservableProperty]
        public partial string FollowStatus { get; set; }

        [ObservableProperty]
        public partial List<bool> Stars { get; set; } = new List<bool>();

        public bool HasScore => IsProduct && !string.IsNullOrEmpty(Score);

        public bool HasStats => !string.IsNullOrEmpty(HotNum) || !string.IsNullOrEmpty(CommentNum);

        public FeedRelationCard(
            FeedRelationKind kind,
            int id,
            string url,
            string title,
            ImageModel logo,
            string score = null,
            string hotNum = null,
            string commentNum = null)
        {
            Kind = kind;
            ID = id;
            Url = url;
            Title = title;
            Logo = logo;

            ResourceLoader feedLoader = ResourceLoader.GetForViewIndependentUse("Feed");
            KindLabel = kind == FeedRelationKind.Topic
                ? feedLoader.GetString("TopicTag")
                : feedLoader.GetString("DigitalTag");

            if (kind == FeedRelationKind.Product)
            {
                ApplyScore(score);
            }

            HotNum = FormatStat(hotNum, "HotNum");
            CommentNum = FormatStat(commentNum, "CommentNum");
            UpdateFollowStatus();
        }

        public static FeedRelationCard FromTargetRow(DyhRowDto row, FeedRelationKind kind)
        {
            if (row == null) { return null; }

            ImageModel logo = string.IsNullOrEmpty(row.Logo)
                ? null
                : new ImageModel(row.Logo, ImageType.Icon);

            return new FeedRelationCard(
                kind: kind,
                id: row.Id > 0 ? row.Id : ParseIdFromUrl(row.Url),
                url: row.Url,
                title: row.Title,
                logo: logo,
                score: row.StarAverageScore,
                hotNum: row.HotNumTxt,
                commentNum: row.FeedCommentNumTxt);
        }

        public static FeedRelationCard FromRelation(RelationRowsItem item)
        {
            if (item == null) { return null; }

            FeedRelationKind kind = item.IsProduct
                ? FeedRelationKind.Product
                : FeedRelationKind.Topic;

            return new FeedRelationCard(
                kind: kind,
                id: item.Id > 0 ? item.Id : ParseIdFromUrl(item.Url),
                url: item.Url,
                title: item.Title,
                logo: item.Logo,
                score: item.StarAverageScore,
                hotNum: item.HotNum,
                commentNum: item.CommentNum);
        }

        public static bool IsProductLink(string url, string entityType = null, string targetType = null)
        {
            if (string.Equals(entityType, "product", StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, "product", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.IsNullOrEmpty(url)) { return false; }

            return url.Contains("/product/", StringComparison.OrdinalIgnoreCase)
                && url.IndexOf("category", StringComparison.OrdinalIgnoreCase) < 0;
        }

        public static bool IsTopicLink(string url, string entityType = null, string targetType = null)
        {
            if (IsProductLink(url, entityType, targetType)) { return false; }

            if (string.Equals(entityType, "topic", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entityType, "tag", StringComparison.OrdinalIgnoreCase)
                || string.Equals(entityType, "feedTopic", StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, "topic", StringComparison.OrdinalIgnoreCase)
                || string.Equals(targetType, "tag", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (string.IsNullOrEmpty(url)) { return false; }

            return url.StartsWith("/t/", StringComparison.OrdinalIgnoreCase)
                || url.Contains("/t/", StringComparison.OrdinalIgnoreCase)
                || url.Contains("/topic/", StringComparison.OrdinalIgnoreCase);
        }

        public void EnsureDetailLoaded()
        {
            if (Interlocked.CompareExchange(ref _detailLoadState, 1, 0) != 0) { return; }
            _ = LoadDetailAsync();
        }

        private async Task LoadDetailAsync()
        {
            try
            {
                if (Kind == FeedRelationKind.Product)
                {
                    await LoadProductDetailAsync();
                }
                else
                {
                    await LoadTopicDetailAsync();
                }
            }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(FeedRelationCard)).LogWarning(ex, ex.ExceptionToMessage());
            }
        }

        private async Task LoadProductDetailAsync()
        {
            Uri uri = ID > 0
                ? UriHelper.GetUri(UriType.GetProductDetail, ID)
                : string.IsNullOrEmpty(Title)
                    ? null
                    : UriHelper.GetUri(UriType.GetProductDetailByName, Title);

            if (uri == null) { return; }

            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(uri, true);
            if (!isSucceed || result == null) { return; }

            ProductDetailDto dto = DtoJson.Deserialize<ProductDetailDto>(result);
            if (dto == null) { return; }

            await WindowContext.DispatcherQueue.EnqueueAsync(() => ApplyProductDetail(dto));
        }

        private async Task LoadTopicDetailAsync()
        {
            if (string.IsNullOrEmpty(Title)) { return; }

            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetTopicDetail, Title), true);
            if (!isSucceed || result == null) { return; }

            TopicDetailDto dto = DtoJson.Deserialize<TopicDetailDto>(result);
            if (dto == null) { return; }

            await WindowContext.DispatcherQueue.EnqueueAsync(() => ApplyTopicDetail(dto));
        }

        private void ApplyProductDetail(ProductDetailDto dto)
        {
            if (dto.RatingAverageScore > 0)
            {
                ApplyScore(dto.RatingAverageScore.ToString("0.#"));
            }

            if (!string.IsNullOrEmpty(dto.HotNumTxt))
            {
                HotNum = FormatStat(dto.HotNumTxt, "HotNum");
            }

            if (!string.IsNullOrEmpty(dto.FeedCommentNumTxt))
            {
                CommentNum = FormatStat(dto.FeedCommentNumTxt, "CommentNum");
            }

            Followed = dto.UserAction?.Follow == 1;
        }

        private void ApplyTopicDetail(TopicDetailDto dto)
        {
            if (!string.IsNullOrEmpty(dto.HotNumTxt))
            {
                HotNum = FormatStat(dto.HotNumTxt, "HotNum");
            }

            if (!string.IsNullOrEmpty(dto.CommentnumTxt))
            {
                CommentNum = FormatStat(dto.CommentnumTxt, "CommentNum");
            }

            Followed = dto.UserAction?.Follow == 1;
        }

        private void ApplyScore(string score)
        {
            if (string.IsNullOrEmpty(score)) { return; }

            Score = score;
            if (double.TryParse(score, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value)
                && value > 0)
            {
                ScoreValue = value;
                int count = Math.Max(0, Math.Min(5, (int)Math.Round(value / 2.0)));
                List<bool> stars = new List<bool>(count);
                for (int i = 0; i < count; i++) { stars.Add(true); }
                Stars = stars;
            }

            OnPropertyChanged(nameof(HasScore));
        }

        partial void OnHotNumChanged(string value) => OnPropertyChanged(nameof(HasStats));

        partial void OnCommentNumChanged(string value) => OnPropertyChanged(nameof(HasStats));

        partial void OnFollowedChanged(bool value) => UpdateFollowStatus();

        private void UpdateFollowStatus()
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("FeedListPage");
            FollowStatus = Followed ? loader.GetString("Unfollow") : loader.GetString("Follow");
        }

        private static string FormatStat(string value, string suffixKey)
        {
            if (string.IsNullOrEmpty(value)) { return string.Empty; }
            if (value.IndexOf("热") >= 0 || value.IndexOf("讨") >= 0 || value.IndexOf("Hot", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return value;
            }

            return value + ResourceLoader.GetForViewIndependentUse("FeedListPage").GetString(suffixKey);
        }

        private static int ParseIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) { return 0; }
            int slash = url.TrimEnd('/').LastIndexOf('/');
            if (slash < 0 || slash >= url.Length - 1) { return 0; }
            return int.TryParse(url.Substring(slash + 1), out int id) ? id : 0;
        }
    }
}
