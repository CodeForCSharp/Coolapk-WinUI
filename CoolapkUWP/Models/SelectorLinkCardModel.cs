using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    [INotifyPropertyChanged]
    internal partial class SelectorLinkCardModel : IndexPageHasEntitiesModel
    {
        [ObservableProperty]
        public partial int SelectedIndex { get; set; }

        private int _nextIndex;

        public SelectorLinkCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.SelectorLink)
        {
            SelectedIndex = 0;
            UpdateSelection();
        }

        public static SelectorLinkCardModel FromJson(JsonObject json)
        {
            SelectorLinkCardModel model = new SelectorLinkCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));
            if (json.TryGetPropertyValue("extraDataArr", out JsonNode extra)
                && extra is JsonObject extraObj
                && extraObj.TryGetPropertyValue("selectedTab", out JsonNode selectedTab)
                && !string.IsNullOrEmpty(selectedTab.ToString()))
            {
                for (int i = 0; i < model.Entities.Count; i++)
                {
                    if (model.Entities[i] is IndexPageModel entity && entity.Title == selectedTab.ToString())
                    {
                        model.SelectedIndex = i;
                        break;
                    }
                }
            }
            return model;
        }

        protected override Entity CreateEntity(JsonObject itemObj, string entityType)
        {
            SelectorLinkModel link = SelectorLinkModel.CreateFromJson(itemObj);
            link.Parent = this;
            link.Index = _nextIndex++;
            return link;
        }

        partial void OnSelectedIndexChanged(int value)
            => UpdateSelection();

        private void UpdateSelection()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i] is SelectorLinkModel link)
                {
                    link.IsSelected = i == SelectedIndex;
                }
            }
        }
    }
}
