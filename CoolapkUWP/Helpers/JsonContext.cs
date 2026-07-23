using CoolapkUWP.Models.Update;
using CoolapkUWP.Models.Upload;
using CommunityToolkit.WinUI.Helpers;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Helpers
{
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
    internal partial class JsonContext : JsonSerializerContext
    {
    }
}
