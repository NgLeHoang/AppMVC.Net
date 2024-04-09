using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using APPMVC.NET.Models.Blog;

namespace APPMVC.NET.Models.Product
{
    [Table("BlogPhoto")]
    public class BlogPhoto
    {
        [Key]
        public int Id { get; set; }

        // abc.png, .jpg ..
        // /contents/Products/abc.png ...
        public string? FileName { get; set; }

        public int PostID { get; set; }
        [ForeignKey("PostID")]
        public Post? Post { get; set; }
    }
}