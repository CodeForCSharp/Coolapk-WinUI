using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CoolapkUWP.Models
{
    public interface IPic
    {
        string Uri { get; }
        ImageType Type { get; }
        BitmapImage Pic { get; }
    }

    /// <summary>
    /// 榜单条目(产品/话题)：logo、标题、热度、讨论数与琥珀色星级评分。
    /// </summary>
    public interface IStarRating
    {
        ImageModel Pic { get; }
        string Title { get; }
        string HotNum { get; }
        string CommentNum { get; }
        string RatingScore { get; }
        List<bool> TargetStars { get; }
        string RightScore { get; }
        string RightLabel { get; }
    }

    public interface IHasTitle
    {
        string Url { get; }
        string Title { get; }
    }

    public interface IHasDescription : IHasTitle
    {
        ImageModel Pic { get; }
        string Description { get; }
    }

    public interface IHasSubtitle : IHasDescription
    {
        string SubTitle { get; }
    }

    public interface ICanCopy
    {
        bool IsCopyEnabled { get; set; }
    }

    public interface ICanLike
    {
        int ID { get; }
        bool Liked { get; set; }
    }

    public interface ICanStar
    {
        int ID { get; }
        bool Stared { get; set; }
        int StarNum { get; set; }
    }

    public interface ICanReply
    {
        int ID { get; }
        int ReplyNum { get; set; }
    }

    public interface ICanFollow
    {
        int ID { get; }
        bool Followed { get; set; }
    }

    public interface IUserModel
    {
        int FansNum { get; }
        int FollowNum { get; }

        string Bio { get; }
        string Url { get; }
        string UserName { get; }
        string LoginTime { get; }

        ImageModel Cover { get; }
        ImageModel UserAvatar { get; }
    }
}
