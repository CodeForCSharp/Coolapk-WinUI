using CoolapkUWP.Helpers;
using System.Text.Json.Nodes;
using System;

namespace CoolapkUWP.Models.Users
{
    public class ContactModel : Entity
    {
        public int DateLine { get; private set; }
        public bool IsFriend { get; private set; }
        public UserModel UserInfo { get; private set; }

        public ContactModel(JsonObject token) : base(token)
        {
            if (token.TryGetPropertyValue("dateline", out JsonNode dateline))
            {
                DateLine = dateline.ToInt32Safe();
            }

            if (token.TryGetPropertyValue("isfriend", out JsonNode isfriend))
            {
                IsFriend = Convert.ToBoolean(isfriend.ToInt32Safe());
            }

            if (token.TryGetPropertyValue("userInfo", out JsonNode v1))
            {
                JsonObject userInfo = v1.AsObject();
                UserInfo = new UserModel(userInfo);
            }
        }

        public override string ToString() => UserInfo.ToString();
    }
}
