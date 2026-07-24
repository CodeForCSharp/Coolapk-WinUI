using CoolapkUWP.Common;
using CoolapkUWP.Models.Update;
using CoolapkUWP.Models.Upload;
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
    [JsonSerializable(typeof(APIVersions))]
    [JsonSerializable(typeof(TokenVersions))]
    [JsonSerializable(typeof(Microsoft.UI.Xaml.ElementTheme))]
    internal partial class JsonContext : JsonSerializerContext
    {
    }
}
