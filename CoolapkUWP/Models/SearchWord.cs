using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    public class SearchWord : Entity
    {
        private readonly SearchWordDto _dto;

        public string Glyph { get; }
        public string Title => _dto.Title;

        public SearchWord(SearchWordDto dto) : base(dto)
        {
            _dto = dto;

            if (dto.Logo != null)
            {
                Glyph = dto.Logo.Contains("app") || dto.Logo.Contains("cube")
                    ? "\uE719"
                    : dto.Logo.Contains("xitongguanli") ? "\uE77B" : "\uE721";
            }
        }

        public static SearchWord FromJson(JsonObject json)
            => new SearchWord(DtoJson.Deserialize<SearchWordDto>(json));

        public override string ToString()
        {
            switch (Glyph)
            {
                case "\uE719":
                case "\uE77B":
                    return Title
                        .Replace("搜索应用：", string.Empty)
                        .Replace("搜索用户：", string.Empty);
                default:
                    return Title;
            }
        }
    }
}
