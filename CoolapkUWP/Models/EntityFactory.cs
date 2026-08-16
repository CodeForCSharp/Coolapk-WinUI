using CoolapkUWP.Models.Feeds;
using CoolapkUWP.Models.Users;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    /// <summary>
    /// 嵌套实体的统一工厂：根据 wire entityType 创建对应的实体模型。
    /// </summary>
    internal static class EntityFactory
    {
        internal static Entity CreateNested(string entityType, JsonObject json)
            => entityType switch
            {
                "feed" => FeedModel.FromJson(json),
                "user" => UserModel.FromJson(json),
                "collection" => CollectionModel.FromJson(json),
                _ => IndexPageModel.FromJson(json),
            };
    }
}
