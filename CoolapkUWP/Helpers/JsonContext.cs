using CoolapkUWP.Common;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Models.Update;
using CoolapkUWP.Models.Upload;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Helpers
{
    [JsonSourceGenerationOptions(UseStringEnumConverter = true)]
    [JsonSerializable(typeof(SearchWordDto))]
    [JsonSerializable(typeof(AppDto))]
    [JsonSerializable(typeof(TopicDetailDto))]
    [JsonSerializable(typeof(DyhDetailDto))]
    [JsonSerializable(typeof(UserDetailDto))]
    [JsonSerializable(typeof(CollectionDetailDto))]
    [JsonSerializable(typeof(ProductDetailDto))]
    [JsonSerializable(typeof(ProfileDetailDto))]
    [JsonSerializable(typeof(UserActionDto))]
    [JsonSerializable(typeof(FeedDto))]
    [JsonSerializable(typeof(FeedReplyDto))]
    [JsonSerializable(typeof(VoteItemDto))]
    [JsonSerializable(typeof(UserDto))]
    [JsonSerializable(typeof(CollectionDto))]
    [JsonSerializable(typeof(TopicDto))]
    [JsonSerializable(typeof(HistoryDto))]
    [JsonSerializable(typeof(IndexPageDto))]
    [JsonSerializable(typeof(IndexPageHasEntitiesDto))]
    [JsonSerializable(typeof(IndexPageMessageCardDto))]
    [JsonSerializable(typeof(IndexPageOperationCardDto))]
    [JsonSerializable(typeof(ContactDto))]
    [JsonSerializable(typeof(NotificationNumbersDto))]
    [JsonSerializable(typeof(NotificationDto))]
    [JsonSerializable(typeof(UpdateInfo))]
    [JsonSerializable(typeof(Asset))]
    [JsonSerializable(typeof(UploadPicturePrepareResult))]
    [JsonSerializable(typeof(UploadPrepareInfo))]
    [JsonSerializable(typeof(UploadFileInfo))]
    [JsonSerializable(typeof(UploadFileFragment))]
    [JsonSerializable(typeof(System.Collections.Generic.List<UploadFileFragment>))]
    [JsonSerializable(typeof(UploadFileFragment[]))]
    [JsonSerializable(typeof(APIVersion))]
    [JsonSerializable(typeof(UserAgent))]
    [JsonSerializable(typeof(APIVersions))]
    [JsonSerializable(typeof(TokenVersions))]
    [JsonSerializable(typeof(Microsoft.UI.Xaml.ElementTheme))]
    [JsonSerializable(typeof(string))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(int))]
    [JsonSerializable(typeof(uint))]
    [JsonSerializable(typeof(System.DateTime))]
    internal partial class JsonContext : JsonSerializerContext
    {
    }
}
