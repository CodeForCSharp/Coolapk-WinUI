using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CoolapkUWP.Services
{
    /// <summary>
    /// 链接卡片预览:请求酷安/哔哩哔哩/IT之家的外链数据并提取为 <see cref="LinkFeedDto"/>。
    /// </summary>
    internal static class LinkPreviewService
    {
        public static async Task<LinkFeedDto> LoadAsync(Uri uri, LinkType type, bool isPost = false, MultipartFormDataContent content = null)
        {
            string result;
            if (isPost) { (_, result) = await RequestHelper.PostStringAsync(uri, content); }
            else { (_, result) = await RequestHelper.GetStringAsync(uri, "XMLHttpRequest"); }

            if (string.IsNullOrEmpty(result)) { return new LinkFeedDto(); }

            JsonObject json;
            try { json = JsonNode.Parse(result).AsObject(); }
            catch { return new LinkFeedDto(); }

            return Extract(json, type);
        }

        private static LinkFeedDto Extract(JsonObject json, LinkType type)
        {
            LinkFeedDto dto = new LinkFeedDto();
            switch (type)
            {
                case LinkType.Coolapk:
                    {
                        if (json.TryGetPropertyValue("data", out JsonNode v1))
                        {
                            JsonObject data = v1.AsObject();
                            if (data.TryGetPropertyValue("userInfo", out JsonNode v2))
                            {
                                JsonObject userInfo = v2.AsObject();
                                dto.UserUrl = userInfo.TryGetPropertyValue("url", out JsonNode uurl) ? uurl.ToString() : null;
                                dto.UserName = userInfo.TryGetPropertyValue("username", out JsonNode username) ? username.ToString() : null;
                            }
                            dto.Url = data.TryGetPropertyValue("url", out JsonNode url) ? url.ToString() : null;
                            dto.Message = data.TryGetPropertyValue("message", out JsonNode message) ? message.ToString() : null;
                            if (data.TryGetPropertyValue("dateline", out JsonNode dateline))
                            {
                                dto.Dateline = long.TryParse(dateline.ToString(), out long datelineValue) ? datelineValue : 0L;
                            }
                            dto.MessageTitle = data.TryGetPropertyValue("message_title", out JsonNode message_title) ? message_title.ToString() : null;
                            if (data.TryGetPropertyValue("picArr", out JsonNode picArr) && picArr.AsArray().Count > 0 && picArr != null)
                            {
                                foreach (JsonNode item in picArr.AsArray())
                                {
                                    (dto.PicUris ??= new System.Collections.Generic.List<string>()).Add(item.ToString());
                                }
                            }
                        }
                    }
                    break;
                case LinkType.Bilibili:
                    {
                        if (json.TryGetPropertyValue("data", out JsonNode v1))
                        {
                            JsonObject data = v1.AsObject();
                            if (data.TryGetPropertyValue("card", out JsonNode v2))
                            {
                                JsonObject card = v2.AsObject();
                                if (card.TryGetPropertyValue("card", out JsonNode v3))
                                {
                                    JsonObject card1 = JsonNode.Parse(v3.ToString()).AsObject();
                                    if (card1.TryGetPropertyValue("item", out JsonNode v4))
                                    {
                                        JsonObject item = v4.AsObject();
                                        dto.Message = item.TryGetPropertyValue("description", out JsonNode description) ? description.ToString() : null;
                                        dto.MessageTitle = item.TryGetPropertyValue("title", out JsonNode title) ? title.ToString() : null;
                                        if (item.TryGetPropertyValue("upload_time", out JsonNode upload_time))
                                        {
                                            dto.Dateline = long.TryParse(upload_time.ToString(), out long uploadTimeValue) ? uploadTimeValue : 0L;
                                        }
                                        if (item.TryGetPropertyValue("pictures", out JsonNode pictures))
                                        {
                                            foreach (JsonNode picturesItem in pictures.AsArray())
                                            {
                                                (dto.PicUris ??= new System.Collections.Generic.List<string>()).Add(picturesItem.AsObject()["img_src"]?.ToString().Replace("\"", string.Empty));
                                            }
                                        }
                                    }
                                    if (card1.TryGetPropertyValue("user", out JsonNode v5))
                                    {
                                        JsonObject user = v5.AsObject();
                                        dto.UserName = user.TryGetPropertyValue("name", out JsonNode name) ? name.ToString() : null;
                                        dto.UserUrl = user.TryGetPropertyValue("uid", out JsonNode uid) ? "https://space.bilibili.com/" + uid.ToString() : null;
                                    }
                                }
                            }
                            if (data.TryGetPropertyValue("dynamic_id_str", out JsonNode dynamic_id_str))
                            {
                                dto.Url = "https://t.bilibili.com/" + dynamic_id_str;
                            }
                        }
                    }
                    break;
                case LinkType.ITHome:
                    {
                        if (json.TryGetPropertyValue("data", out JsonNode v1))
                        {
                            JsonObject data = v1.AsObject();
                            if (data.TryGetPropertyValue("id", out JsonNode id))
                            {
                                dto.Url = $"ithome://qcontent?id={id.ToString().Replace("\"", string.Empty)}";
                            }
                            if (data.TryGetPropertyValue("contents", out JsonNode contents))
                            {
                                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                                foreach (JsonNode v in contents.AsArray())
                                {
                                    JsonObject vObj = v.AsObject();
                                    if (vObj.TryGetPropertyValue("content", out JsonNode content) && vObj.TryGetPropertyValue("type", out JsonNode type2))
                                    {
                                        switch (type2.ToString())
                                        {
                                            case "0":
                                                builder.Append(content.ToString());
                                                break;
                                            case "2":
                                                if (vObj.TryGetPropertyValue("link", out JsonNode link) && !string.IsNullOrEmpty(link.ToString()))
                                                { builder.Append("<a class=\"feed-link-url\" href=\"" + link.ToString() + "\" target=\"_blank\" rel=\"nofollow\">查看链接</a>"); }
                                                else { builder.Append(content.ToString()); }
                                                break;
                                            case "3":
                                                if (vObj.TryGetPropertyValue("topicId", out JsonNode topicId) && !string.IsNullOrEmpty(topicId.ToString()))
                                                { builder.Append("<a class=\"feed-link-tag\" href=\"" + "ithome://qtopic?id=" + topicId.ToString() + "\">" + content.ToString() + "</a>"); }
                                                else { builder.Append(content.ToString()); }
                                                break;
                                            default:
                                                builder.Append(content.ToString());
                                                break;
                                        }
                                    }
                                }
                                dto.Message = builder.ToString();
                            }
                            if (data.TryGetPropertyValue("user", out JsonNode v2))
                            {
                                JsonObject user = v2.AsObject();
                                dto.UserName = user.TryGetPropertyValue("userNick", out JsonNode userNick) ? userNick.ToString() : null;
                            }
                            if (data.TryGetPropertyValue("pictures", out JsonNode pictures))
                            {
                                foreach (JsonNode item in pictures.AsArray())
                                {
                                    (dto.PicUris ??= new System.Collections.Generic.List<string>()).Add(item.AsObject()["src"]?.ToString());
                                }
                            }
                            if (data.TryGetPropertyValue("createTime", out JsonNode createTime))
                            {
                                dto.Dateline = System.DateTime.TryParse(createTime.ToString(), out System.DateTime createDateTime)
                                    ? System.Convert.ToInt64(createDateTime.ConvertDateTimeToUnixTimeStamp())
                                    : 0L;
                            }
                        }
                    }
                    break;
                default: break;
            }
            dto.Succeed = true;
            return dto;
        }
    }
}
