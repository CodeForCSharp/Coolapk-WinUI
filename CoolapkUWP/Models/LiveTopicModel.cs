using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models
{
    /// <summary>
    /// 直播条目（发布会直播），展示封面、标题、主播、时间与观看数。
    /// </summary>
    public class LiveTopicModel : Entity, IHasDescription
    {
        public int ID { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public ImageModel Pic { get; private set; }
        public string Url => $"/live/{ID}";

        public LiveTopicModel(LiveTopicDto dto) : base(dto)
        {

            ID = dto.Id;
            Title = dto.Title;

            Description = BuildDescription(dto);

            if (!string.IsNullOrEmpty(dto.PicUrl))
            {
                Pic = new ImageModel(dto.PicUrl, ImageType.SmallImage);
            }
        }

        private static string BuildDescription(LiveTopicDto dto)
        {
            List<string> parts = new List<string>();
            string presenter = dto.UserInfo?.Username;
            if (!string.IsNullOrEmpty(presenter)) { parts.Add(presenter); }
            if (!string.IsNullOrEmpty(dto.ShowLiveTime)) { parts.Add(dto.ShowLiveTime); }
            if (!string.IsNullOrEmpty(dto.VisitNumFormat)) { parts.Add($"{dto.VisitNumFormat}观看"); }
            return parts.Count > 0 ? string.Join(" · ", parts) : dto.Description;
        }

        public static LiveTopicModel FromJson(JsonObject json)
            => new LiveTopicModel(DtoJson.Deserialize<LiveTopicDto>(json));

        public override string ToString() => $"{Title} - {Description}";
    }
}
