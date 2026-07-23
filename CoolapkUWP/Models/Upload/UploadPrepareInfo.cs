using System.Text.Json.Serialization;

namespace CoolapkUWP.Models.Upload
{
    public class UploadPrepareInfo
    {
        [JsonPropertyName("accessKeySecret")]
        public string AccessKeySecret { get; set; }

        [JsonPropertyName("accessKeyId")]
        public string AccessKeyID { get; set; }

        [JsonPropertyName("securityToken")]
        public string SecurityToken { get; set; }

        [JsonPropertyName("expiration")]
        public string Expiration { get; set; }

        [JsonPropertyName("uploadImagePrefix")]
        public string UploadImagePrefix { get; set; }

        [JsonPropertyName("endPoint")]
        public string EndPoint { get; set; }

        [JsonPropertyName("bucket")]
        public string Bucket { get; set; }

        [JsonPropertyName("callbackUrl")]
        public string CallbackUrl { get; set; }
    }
}
