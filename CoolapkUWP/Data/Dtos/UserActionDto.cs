namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户操作状态(userAction),字段为 0/1 数字或 "0"/"1" 字符串。
    /// </summary>
    public class UserActionDto
    {
        public string Follow { get; set; }
        public string Like { get; set; }
    }
}
