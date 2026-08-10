using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;

using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models.Feeds
{
    public partial class LinkFeedModel : INotifyPropertyChanged
    {
        private string url;
        public string Url
        {
            get => url;
            set
            {
                url = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool succeed;
        public bool Succeed
        {
            get => succeed;
            set
            {
                succeed = value;
                RaisePropertyChangedEvent();
            }
        }

        private string message;
        public string Message
        {
            get => message;
            set
            {
                message = value;
                RaisePropertyChangedEvent();
            }
        }

        private string messageTitle;
        public string MessageTitle
        {
            get => messageTitle;
            set
            {
                messageTitle = value;
                RaisePropertyChangedEvent();
            }
        }

        private string dateline;
        public string Dateline
        {
            get => dateline;
            set
            {
                dateline = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool showPicArr;
        public bool ShowPicArr
        {
            get => showPicArr;
            set
            {
                showPicArr = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool showUser = true;
        public bool ShowUser
        {
            get => showUser;
            set
            {
                if (showUser != value)
                {
                    showUser = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isCopyEnabled;
        public bool IsCopyEnabled
        {
            get => isCopyEnabled;
            set
            {
                if (isCopyEnabled != value)
                {
                    isCopyEnabled = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private List<ImageModel> picArr;
        public List<ImageModel> PicArr
        {
            get => picArr;
            set
            {
                picArr = value;
                RaisePropertyChangedEvent();
            }
        }

        private LinkUserModel userInfo;
        public LinkUserModel UserInfo
        {
            get => userInfo;
            set
            {
                userInfo = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public LinkFeedModel(Uri uri, LinkType type, bool isPost = false, MultipartFormDataContent content = null)
        {
            PicArr = new List<ImageModel>();
            if (!string.IsNullOrEmpty(uri.ToString())) { GetJson(uri, type, isPost, content); }
        }

        private async void GetJson(Uri uri, LinkType type, bool isPost, MultipartFormDataContent content)
        {
            bool isSucceed;
            string result;
            if (isPost) { (isSucceed, result) = await RequestHelper.PostStringAsync(uri, content); }
            else { (isSucceed, result) = await RequestHelper.GetStringAsync(uri, "XMLHttpRequest"); }
            if (isSucceed && !string.IsNullOrEmpty(result))
            {
                JsonObject json = JsonNode.Parse(result).AsObject();
                ReadJson(json, type);
            }
        }

        private void ReadJson(JsonObject json, LinkType type)
        {
            ResourceLoader loader = ResourceLoader.GetForViewIndependentUse("Feed");
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
                                LinkUserModel UserModel = new LinkUserModel();
                                if (userInfo.TryGetPropertyValue("url", out JsonNode uurl))
                                {
                                    UserModel.Url = uurl.ToString();
                                }
                                if (userInfo.TryGetPropertyValue("username", out JsonNode username))
                                {
                                    UserModel.UserName = username.ToString();
                                }
                                UserInfo = UserModel;
                            }
                            if (data.TryGetPropertyValue("url", out JsonNode url))
                            {
                                Url = url.ToString();
                            }
                            if (data.TryGetPropertyValue("feedType", out JsonNode feedType) && feedType.ToString() == "feedArticle")
                            {
                                if (data.TryGetPropertyValue("message", out JsonNode message))
                                {
                                    Message = message.ToString();
                                    if (Message.Contains("</a>") ? Message.Length - 200 >= 7 : Message.Length - 120 >= 7)
                                    {
                                        Message = message.ToString().Substring(0, 120);
                                        Message = Message.Contains("</a>") ? message.ToString().Substring(0, 200) + "...<a href=\"" + Url + "\">" + loader.GetString("ReadMore") + "</a>" : Message + "...<a href=\"" + Url + "\">" + loader.GetString("ReadMore") + "</a>";
                                    }
                                }
                            }
                            else
                            {
                                if (data.TryGetPropertyValue("message", out JsonNode message))
                                {
                                    Message = message.ToString();
                                }
                            }
                            if (data.TryGetPropertyValue("dateline", out JsonNode dateline))
                            {
                                Dateline = dateline.ToInt64Safe().ConvertUnixTimeStampToReadable();
                            }
                            if (data.TryGetPropertyValue("message_title", out JsonNode message_title))
                            {
                                MessageTitle = message_title.ToString();
                            }
                            ShowPicArr = data.TryGetPropertyValue("picArr", out JsonNode picArr) && picArr.AsArray().Count > 0 && picArr != null;
                            if (ShowPicArr)
                            {
                                PicArr = (from item in picArr.AsArray()
                                          select new ImageModel(item.ToString(), ImageType.Icon)).ToList();

                                foreach (ImageModel item in PicArr)
                                {
                                    item.ContextArray = PicArr;
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
                                        if (item.TryGetPropertyValue("description", out JsonNode description))
                                        {
                                            Message = description.ToString();
                                        }
                                        if (item.TryGetPropertyValue("title", out JsonNode title))
                                        {
                                            MessageTitle = title.ToString();
                                        }
                                        if (item.TryGetPropertyValue("upload_time", out JsonNode upload_time))
                                        {
                                            Dateline = upload_time.ToInt64Safe().ConvertUnixTimeStampToReadable();
                                        }
                                        if (item.TryGetPropertyValue("pictures", out JsonNode pictures))
                                        {
                                            ShowPicArr = pictures.AsArray().Count > 0;
                                            PicArr = (from items in pictures.AsArray()
                                                      select new ImageModel(items.AsObject()["img_src"]?.GetValue<string>().Replace("\"", string.Empty), ImageType.OriginImage)).ToList();
                                            foreach (ImageModel items in PicArr)
                                            {
                                                items.ContextArray = PicArr;
                                            }
                                        }
                                    }
                                    if (card1.TryGetPropertyValue("user", out JsonNode v5))
                                    {
                                        JsonObject user = v5.AsObject();
                                        LinkUserModel UserModel = new LinkUserModel();
                                        if (user.TryGetPropertyValue("name", out JsonNode name))
                                        {
                                            UserModel.UserName = name.ToString();
                                        }
                                        if (user.TryGetPropertyValue("uid", out JsonNode uid))
                                        {
                                            UserModel.Url = "https://space.bilibili.com/" + uid.ToString();
                                        }
                                        UserInfo = UserModel;
                                    }
                                }
                            }
                            if (data.TryGetPropertyValue("desc", out JsonNode v6))
                            {
                                JsonObject desc = v6.AsObject();
                                if (data.TryGetPropertyValue("dynamic_id_str", out JsonNode dynamic_id_str))
                                {
                                    Url = "https://t.bilibili.com/" + dynamic_id_str;
                                }
                            }
                            if (Message != null && Message.Length - 120 >= 7)
                            {
                                Message = message.ToString().Substring(0, 120) + "...<a href=\"" + Url + "\">";
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
                                Url = $"ithome://qcontent?id={id.ToString().Replace("\"", string.Empty)}";
                            }
                            if (data.TryGetPropertyValue("contents", out JsonNode contents))
                            {
                                foreach (JsonNode v in contents.AsArray())
                                {
                                    JsonObject vObj = v.AsObject();
                                    if (vObj.TryGetPropertyValue("content", out JsonNode content) && vObj.TryGetPropertyValue("type", out JsonNode type2))
                                    {
                                        switch (type2.ToString())
                                        {
                                            case "0":
                                                Message += content.ToString();
                                                break;
                                            case "2":
                                                if (vObj.TryGetPropertyValue("link", out JsonNode link) && !string.IsNullOrEmpty(link.ToString()))
                                                { Message += "<a class=\"feed-link-url\" href=\"" + link.ToString() + "\" target=\"_blank\" rel=\"nofollow\">查看链接</a>"; }
                                                else { Message += content.ToString(); }
                                                break;
                                            case "3":
                                                if (vObj.TryGetPropertyValue("topicId", out JsonNode topicId) && !string.IsNullOrEmpty(topicId.ToString()))
                                                { Message += "<a class=\"feed-link-tag\" href=\"" + "ithome://qtopic?id=" + topicId.ToString() + "\">" + content.ToString() + "</a>"; }
                                                else { Message += content.ToString(); }
                                                break;
                                            default:
                                                Message += content.ToString();
                                                break;
                                        }
                                    }
                                }
                                if (Message != null && Message.Length - 120 >= 7)
                                {
                                    Message = message.ToString().Substring(0, 120) + "...<a href=\"" + Url + "\">";
                                }
                            }
                            if (data.TryGetPropertyValue("user", out JsonNode v2))
                            {
                                JsonObject user = v2.AsObject();
                                LinkUserModel UserModel = new LinkUserModel();
                                if (user.TryGetPropertyValue("userNick", out JsonNode userNick))
                                {
                                    UserModel.UserName = userNick.ToString();
                                }
                                UserInfo = UserModel;
                            }
                            if (data.TryGetPropertyValue("pictures", out JsonNode pictures))
                            {
                                ShowPicArr = pictures.AsArray().Count > 0;
                                PicArr = (from item in pictures.AsArray()
                                          select new ImageModel(item.AsObject()["src"]?.GetValue<string>(), ImageType.OriginImage)).ToList();
                                foreach (ImageModel item in PicArr)
                                {
                                    item.ContextArray = PicArr;
                                }
                            }
                            if (data.TryGetPropertyValue("createTime", out JsonNode createTime))
                            {
                                Dateline = Convert.ToInt64(Convert.ToDateTime(createTime.ToString()).ConvertDateTimeToUnixTimeStamp()).ConvertUnixTimeStampToReadable();
                            }
                        }
                    }
                    break;
                default: break;
            }
            Succeed = true;
        }
    }
}
