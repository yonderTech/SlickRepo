using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebTest.DB.Models
{
    public interface IBaseModelGuid<out T>
    {
        Guid Id { get; set; }

    }

    
    public class BaseModelGuid 
    {
        public BaseModelGuid()
        {

        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column("Id")]
        public Guid Id { get; set; }
    }
    
    public class BaseModelInt
    {
        public BaseModelInt()
        {

        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key, Column("Id")]
        public int Id { get; set; }
    }
}
