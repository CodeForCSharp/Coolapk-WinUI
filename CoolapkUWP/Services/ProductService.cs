using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Pages.FeedPages;
using CoolapkUWP.ViewModels.FeedPages;
using CommunityToolkit.WinUI;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;

namespace CoolapkUWP.Services
{
    /// <summary>
    /// 产品信息查询与跳转。
    /// </summary>
    internal static class ProductService
    {
        /// <summary>
        /// 根据动态中的设备名查询产品详情并跳转到产品页。
        /// </summary>
        public static async Task NavigateToProductAsync(DependencyObject host, Hyperlink sender)
        {
            UIHelper.ShowProgressBar();
            string device = (sender.Inlines.FirstOrDefault()?.ElementStart?.VisualParent?.DataContext as FeedModelBase)?.DeviceTitle;
            (bool isSucceed, JsonNode result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetProductDetailByName, device), true);
            UIHelper.HideProgressBar();
            if (!isSucceed) { return; }

            ProductDetailDto dto = JsonSerializer.Deserialize<ProductDetailDto>(result, DtoJson.Options);

            if (!string.IsNullOrEmpty(dto.Id))
            {
                FeedListViewModel provider = FeedListViewModel.GetProvider(FeedListType.ProductPageList, dto.Id);

                if (provider != null)
                {
                    _ = host.NavigateAsync(typeof(FeedListPage), provider);
                }
            }
        }
    }
}
