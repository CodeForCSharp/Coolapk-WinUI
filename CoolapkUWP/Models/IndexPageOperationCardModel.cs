using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    internal class IndexPageOperationCardModel : Entity, IHasTitle
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string EntityTemplate { get; private set; }
        public OperationType OperationType { get; private set; }

        public IndexPageOperationCardModel(IndexPageOperationCardDto dto, OperationType type)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            OperationType = type;
            Title = dto.Title;

            switch (type)
            {
                case OperationType.ShowTitle when !string.IsNullOrEmpty(dto.Url):
                    Url = dto.Url;
                    break;

                case OperationType.Refresh:
                    Url = "Refresh";
                    break;

                case OperationType.Login:
                    Url = "Login";
                    break;
            }
        }

        public static IndexPageOperationCardModel FromJson(JsonObject json, OperationType type)
            => new IndexPageOperationCardModel(DtoJson.Deserialize<IndexPageOperationCardDto>(json), type);

        public override string ToString() => Title;
    }
}
