using System.Text.Json.Serialization;

namespace CoolapkUWP.Models.Upload
{
    public class UploadFileInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("resolution")]
        public string Resolution { get; set; }

        [JsonPropertyName("md5")]
        public string MD5 { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("uploadFileName")]
        public string UploadFileName { get; set; }
    }
}
