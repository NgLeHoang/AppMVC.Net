using System.ComponentModel.DataAnnotations;
using APPMVC.NET.Models.Blog;

namespace APPMVC.NET.Areas.Blog.Models
{
    public class CreatePostModel : Post 
    {
        [Display(Name = "Chuyên mục")]
        public int[]? CategoryIDs { get; set; }
    }
}