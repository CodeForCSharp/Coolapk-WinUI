using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Images;
using CoolapkUWP.Models.Users;
using Microsoft.Extensions.Logging;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace CoolapkUWP.Models
{
    internal class IndexPageOperationCardModel : Entity, IHasTitle
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string EntityTemplate { get; private set; }
        public OperationType OperationType { get; private set; }

        public IndexPageOperationCardModel(JsonObject token, OperationType type) : base(token)
        {
            OperationType = type;

            if (token.TryGetPropertyValue("title", out JsonNode title))
            {
                Title = title.ToString();
            }

            switch (type)
            {
                case OperationType.ShowTitle when token.TryGetPropertyValue("url", out JsonNode v3) && !string.IsNullOrEmpty(v3.ToString()):
                    Url = v3.ToString();
                    break;

                case OperationType.Refresh:
                    Url = "Refresh";
                    break;

                case OperationType.Login:
                    Url = "Login";
                    break;
            }
        }

        public override string ToString() => Title;
    }
}
