namespace CoolapkUWP.Data.Dtos
{
    /// <summary>
    /// 用户操作状态(userAction),字段为 0/1 数字或 "0"/"1" 字符串。
    /// </summary>
    public class UserActionDto
    {
        public int Follow { get; set; }
        public int Like { get; set; }
        public int Favorite { get; set; }
        public int Collect { get; set; }
        public int FollowAuthor { get; set; }
        public int AuthorFollowYou { get; set; }
    }
}
