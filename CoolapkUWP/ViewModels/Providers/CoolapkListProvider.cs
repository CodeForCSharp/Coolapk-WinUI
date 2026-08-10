using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using System.Text.Json.Nodes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.Providers
{
    public class CoolapkListProvider
    {
        private readonly string _idName;
        private string _firstItem, _lastItem;
        private readonly Func<int, string, string, Uri> _getUri;

        public Func<JsonObject, IEnumerable<Entity>> GetEntities { get; }

        public CoolapkListProvider(Func<int, string, string, Uri> getUri, Func<JsonObject, IEnumerable<Entity>> getEntities, string idName)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            GetEntities = getEntities ?? throw new ArgumentNullException(nameof(getEntities));
            _idName = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空") : idName;
        }

        public void Clear() => _lastItem = _firstItem = string.Empty;

        public async Task GetEntity(ICollection<Entity> models, int p = 1)
        {
            if (p == 1) { Clear(); }
            (bool isSucceed, JsonNode result) result = await RequestHelper.GetDataAsync(_getUri(p, _firstItem, _lastItem), false);
            if (!result.isSucceed) { return; }

            JsonArray array = result.result.AsArray();
            if (array.Count < 1) { return; }
            if (string.IsNullOrEmpty(_firstItem))
            {
                _firstItem = RequestHelper.GetId(array[0], _idName);
            }
            _lastItem = RequestHelper.GetId(array[^1], _idName);
            foreach (JsonNode item in array)
            {
                IEnumerable<Entity> entities = GetEntities(item.AsObject());
                if (entities == null) { continue; }

                foreach (Entity entity in entities)
                {
                    if (entity == null) { continue; }
                    models.Add(entity);
                }
            }
        }
    }
}
