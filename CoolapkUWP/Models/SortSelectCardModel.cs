using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    [INotifyPropertyChanged]
    internal partial class SortSelectCardModel : IndexPageHasEntitiesModel
    {
        [ObservableProperty]
        public partial int SelectedIndex { get; set; }

        private int _nextIndex;

        public SortSelectCardModel(IndexPageHasEntitiesDto dto)
            : base(dto, global::CoolapkUWP.Models.EntityType.Others)
        {
            SelectedIndex = 0;
            UpdateSelection();
        }

        public static SortSelectCardModel FromJson(JsonObject json)
            => new SortSelectCardModel(DtoJson.Deserialize<IndexPageHasEntitiesDto>(json));

        protected override Entity CreateEntity(JsonObject itemObj, string entityType)
        {
            SortSelectOptionModel opt = SortSelectOptionModel.CreateFromJson(itemObj);
            opt.Parent = this;
            opt.Index = _nextIndex++;
            return opt;
        }

        partial void OnSelectedIndexChanged(int value)
            => UpdateSelection();

        private void UpdateSelection()
        {
            for (int i = 0; i < Entities.Count; i++)
            {
                if (Entities[i] is SortSelectOptionModel option)
                {
                    option.IsSelected = i == SelectedIndex;
                }
            }
        }
    }
}
