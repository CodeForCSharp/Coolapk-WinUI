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
    internal enum EntityType
    {
        Image,
        Others,
        TabLink,
        IconLink,
        TextLinks,
        GridLink,
        SelectorLink,
    }

}
