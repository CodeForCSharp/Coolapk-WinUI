using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoolapkUWP.Models.Upload
{
    public class UploadPicturePrepareResult
    {
        [JsonPropertyName("fileInfo")]
        public IList<UploadFileInfo> FileInfo { get; set; }

        [JsonPropertyName("uploadPrepareInfo")]
        public UploadPrepareInfo UploadPrepareInfo { get; set; }
    }
}
