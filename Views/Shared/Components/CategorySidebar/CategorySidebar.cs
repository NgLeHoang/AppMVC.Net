using APPMVC.NET.Models.Blog;
using Microsoft.AspNetCore.Mvc;

namespace APPMVC.NET.Components
{
    [ViewComponent]
    public class CategorySidebar : ViewComponent
    {
        
        public class CategorySidebarData
        {
            public List<Category>? Categories { get; set; }
            public int Level { get; set; }
            public string? CategorySlug { get; set; }
        }
        public IViewComponentResult Invoke(CategorySidebarData data)
        {
            return View(data);
        }
    }
}