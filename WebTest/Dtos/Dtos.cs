namespace WebTest.Dtos
{
    public class BaseDtoGuid
    {
        public BaseDtoGuid()
        {
            
        }
        public Guid Id { get; set; }
    }

    public class BaseDtoInt
    {
        public BaseDtoInt()
        {
            
        }
        public int Id { get; set; }

    }

    public class User : BaseDtoGuid
    {
        public string Email { get; set; }
    }


    public class Post : BaseDtoInt
    {
        public string Text { get; set; }
    }
}
