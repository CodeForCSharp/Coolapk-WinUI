using System.Text.Json.Nodes;
using System;

namespace CoolapkUWP.Models.Exceptions
{
    public sealed class CoolapkMessageException : Exception
    {
        public const string RequestCaptcha = "err_request_captcha";

        public string MessageStatus { get; }

        public CoolapkMessageException(string message) : base(message) { }

        public CoolapkMessageException(string message, Exception innerException) : base(message, innerException) { }

        public CoolapkMessageException(JsonObject o) : base(o?["message"]?.ToString() ?? string.Empty)
        {
            if (o != null && o.TryGetPropertyValue("messageStatus", out JsonNode token))
            {
                MessageStatus = token.ToString();
            }
        }

        public CoolapkMessageException(JsonObject o, Exception innerException) : base(o?["message"]?.ToString() ?? string.Empty, innerException)
        {
            if (o != null && o.TryGetPropertyValue("messageStatus", out JsonNode token))
            {
                MessageStatus = token.ToString();
            }
        }
    }
}
