using CoolapkUWP.Data;
using CoolapkUWP.Data.Dtos;
using CoolapkUWP.Models.Users;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace CoolapkUWP.Models.Users
{
    public class ContactModel : Entity
    {
        public int DateLine { get; private set; }
        public bool IsFriend { get; private set; }
        public UserModel UserInfo { get; private set; }

        public ContactModel(ContactDto dto)
        {
            InitializeEntity(dto.EntityId, dto.EntityType, dto.EntityForward, dto.EntityFixed);

            DateLine = dto.Dateline.ToInt32Safe();
            IsFriend = dto.Isfriend.ToInt32Safe() != 0;

            if (dto.UserInfo != null)
            {
                UserInfo = new UserModel(dto.UserInfo);
            }
        }

        public static ContactModel FromJson(JsonObject json)
            => new ContactModel(JsonSerializer.Deserialize<ContactDto>(json, DtoJson.Options));

        public override string ToString() => UserInfo.ToString();
    }
}
