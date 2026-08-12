namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 联系人条目。
    /// </summary>
    public class ContactDto : EntityDto
    {
        public string Dateline { get; set; }
        public string Isfriend { get; set; }
        public UserDto UserInfo { get; set; }
    }
}
