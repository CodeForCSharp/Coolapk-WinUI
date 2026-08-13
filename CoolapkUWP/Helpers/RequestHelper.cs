using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using CoolapkUWP.Data;
using CoolapkUWP.Models.Upload;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Helpers
{
    public static class RequestHelper
    {
        private static readonly JsonContext _jsonContext = JsonContext.Default;

        public static async Task<(bool isSucceed, JsonNode result)> GetDataAsync(Uri uri, bool isBackground = false)
        {
            string results = await NetworkHelper.GetStringAsync(uri, "XMLHttpRequest", isBackground);
            if (string.IsNullOrEmpty(results)) { return (false, null); }
            JsonObject token;
            try { token = JsonNode.Parse(results).AsObject(); }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(RequestHelper)).LogError(ex, ex.ExceptionToMessage());
                MessageHelper.ShowMessage("加载失败");
                return (false, null);
            }
            if (!token.TryGetPropertyValue("data", out JsonNode data) && token.TryGetPropertyValue("message", out JsonNode message))
            {
                MessageHelper.ShowMessage(message.ToString());
                return (false, null);
            }
            else { return (data != null && !string.IsNullOrWhiteSpace(data.ToString()), data); }
        }

        public static async Task<(bool isSucceed, string result)> GetStringAsync(Uri uri, string request = "com.coolapk.market", bool isBackground = false)
        {
            string results = await NetworkHelper.GetStringAsync(uri, request, isBackground);
            if (string.IsNullOrWhiteSpace(results))
            {
                MessageHelper.ShowMessage("加载失败");
                return (false, results);
            }
            else { return (true, results); }
        }

        public static async Task<(bool isSucceed, JsonNode result)> PostDataAsync(Uri uri, HttpContent content = null, bool isBackground = false)
        {
            string json = await NetworkHelper.PostAsync(uri, content, isBackground);
            if (string.IsNullOrEmpty(json)) { return (false, null); }
            JsonObject token;
            try { token = JsonNode.Parse(json).AsObject(); }
            catch (Exception ex)
            {
                SettingsHelper.LogManager.CreateLogger(nameof(RequestHelper)).LogError(ex, ex.ExceptionToMessage());
                MessageHelper.ShowMessage("加载失败");
                return (false, null);
            }
            if (!token.TryGetPropertyValue("data", out JsonNode data) && token.TryGetPropertyValue("message", out JsonNode message))
            {
                bool _isSucceed = token.TryGetPropertyValue("error", out JsonNode error) && error.ToInt32Safe() == 0;
                MessageHelper.ShowMessage(message.ToString());
                return (_isSucceed, token);
            }
            else
            {
                return data != null && !string.IsNullOrWhiteSpace(data.ToString())
                ? ((bool isSucceed, JsonNode result))(true, data)
                : ((bool isSucceed, JsonNode result))(token != null && !string.IsNullOrEmpty(token.ToString()), token);
            }
        }

        public static async Task<(bool isSucceed, string result)> PostStringAsync(Uri uri, HttpContent content = null, bool isBackground = false)
        {
            string json = await NetworkHelper.PostAsync(uri, content, isBackground);
            if (string.IsNullOrEmpty(json))
            {
                MessageHelper.ShowMessage("加载失败");
                return (false, null);
            }
            else { return (true, json); }
        }

        public static string GetId(JsonNode token, string _idName)
        {
            return token == null
                ? string.Empty
                : token.AsObject().TryGetPropertyValue(_idName, out JsonNode jToken)
                    ? jToken.ToString()
                    : token.AsObject().TryGetPropertyValue("entityId", out JsonNode v1)
                        ? v1.ToString()
                        : token.AsObject().TryGetPropertyValue("id", out JsonNode v2)
                            ? v2.ToString()
                            : throw new ArgumentException(nameof(_idName));
        }

        public static async Task<List<string>> UploadImages(IEnumerable<UploadFileFragment> images)
        {
            List<string> responses = new List<string>();
            using (MultipartFormDataContent content = new MultipartFormDataContent())
            {
                string json = JsonSerializer.Serialize(images, _jsonContext.UploadFileFragmentArray);
                using (StringContent uploadBucket = new StringContent("image"))
                using (StringContent uploadDir = new StringContent("feed"))
                using (StringContent is_anonymous = new StringContent("0"))
                using (StringContent uploadFileList = new StringContent(json))
                {
                    content.Add(uploadBucket, "uploadBucket");
                    content.Add(uploadDir, "uploadDir");
                    content.Add(is_anonymous, "is_anonymous");
                    content.Add(uploadFileList, "uploadFileList");
                    (bool isSucceed, JsonNode result) = await PostDataAsync(UriHelper.GetUri(UriType.OSSUploadPrepare), content);
                    if (isSucceed)
                    {
                        UploadPicturePrepareResult data = result.Deserialize(_jsonContext.UploadPicturePrepareResult);
                        Dictionary<string, UploadFileFragment> imageMap = new Dictionary<string, UploadFileFragment>();
                        foreach (UploadFileFragment fragment in images)
                        {
                            imageMap[fragment.MD5] = fragment;
                        }
                        foreach (UploadFileInfo info in data.FileInfo)
                        {
                            if (!imageMap.TryGetValue(info.MD5, out UploadFileFragment image)) { continue; }
                            using (Stream stream = image.Bytes.GetStream())
                            {
                                string response = await Task.Run(() => OSSUploadHelper.OssUpload(data.UploadPrepareInfo, info, stream, "image/png"));
                                if (!string.IsNullOrEmpty(response))
                                {
                                    try
                                    {
                                        JsonObject token = JsonNode.Parse(response).AsObject();
                                        if (token.TryGetPropertyValue("data", out JsonNode value)
                                            && value.AsObject().TryGetPropertyValue("url", out JsonNode url)
                                            && !string.IsNullOrEmpty(url.ToString()))
                                        {
                                            responses.Add(url.ToString());
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        SettingsHelper.LogManager.CreateLogger(nameof(RequestHelper)).LogError(ex, ex.ExceptionToMessage());
                                        MessageHelper.ShowMessage("上传失败");
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return responses;
        }

        public static async Task<bool> CheckLogin()
        {
            (bool isSucceed, _) = await GetDataAsync(UriHelper.GetUri(UriType.CheckLoginInfo), true);
            return isSucceed;
        }
    }
}
