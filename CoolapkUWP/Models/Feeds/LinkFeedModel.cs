using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Feeds
{
    public partial class LinkFeedModel : ObservableObject
    {
        [ObservableProperty]
        public partial string Url { get; set; }

        [ObservableProperty]
        public partial bool Succeed { get; set; }

        [ObservableProperty]
        public partial string Message { get; set; }

        [ObservableProperty]
        public partial string MessageTitle { get; set; }

        [ObservableProperty]
        public partial string Dateline { get; set; }

        [ObservableProperty]
        public partial bool ShowPicArr { get; set; }

        [ObservableProperty]
        public partial bool ShowUser { get; set; } = true;

        [ObservableProperty]
        public partial bool IsCopyEnabled { get; set; }

        [ObservableProperty]
        public partial List<ImageModel> PicArr { get; set; } = new List<ImageModel>();

        [ObservableProperty]
        public partial LinkUserModel UserInfo { get; set; }

        public LinkFeedModel(LinkFeedDto dto, LinkType type)
        {
            Url = dto.Url;
            Succeed = dto.Succeed;
            MessageTitle = dto.MessageTitle;

            if (dto.Dateline != null)
            {
                Dateline = dto.Dateline.ToInt64Safe().ConvertUnixTimeStampToReadable();
            }

            if (dto.UserUrl != null)
            {
                UserInfo = new LinkUserModel { Url = dto.UserUrl, UserName = dto.UserName };
            }

            if (dto.PicUris != null && dto.PicUris.Count > 0)
            {
                ShowPicArr = true;
                PicArr = dto.PicUris
                    .Select(x => !string.IsNullOrEmpty(x) ? new ImageModel(x, ImageType.OriginImage) : null)
                    .Where(x => x != null).ToList();

                foreach (ImageModel item in PicArr)
                {
                    item.ContextArray = PicArr;
                }
            }

            if (dto.Message != null)
            {
                Message = FormatMessage(dto.Message, dto.Url, type);
            }
        }

        private static string FormatMessage(string message, string url, LinkType type)
        {
            if (type == LinkType.Coolapk)
            {
                if (message.Contains("</a>") ? message.Length - 200 >= 7 : message.Length - 120 >= 7)
                {
                    ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");
                    string readMore = "...<a href=\"" + url + "\">" + loader.GetString("ReadMore") + "</a>";
                    return message.Contains("</a>")
                        ? message.Substring(0, 200) + readMore
                        : message.Substring(0, 120) + readMore;
                }
                return message;
            }
            else if (type == LinkType.Bilibili || type == LinkType.ITHome)
            {
                if (message.Length - 120 >= 7)
                {
                    return message.Substring(0, 120) + "...<a href=\"" + url + "\">";
                }
                return message;
            }
            return message;
        }
    }
}
