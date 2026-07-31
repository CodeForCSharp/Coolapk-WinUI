using CoolapkUWP.Common;
using CoolapkUWP.Models.Update;
using CoolapkUWP.Models.Upload;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Helpers
{
    [JsonSourceGenerationOptions(UseStringEnumConverter = true)]
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
