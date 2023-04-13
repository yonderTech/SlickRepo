namespace WebTest.DB.Models
{
    public class User : BaseModelGuid
    {
        public string Email { get; set; }
    }


    public class Post: BaseModelInt
    {
        public string Text { get; set; }
    }
}
