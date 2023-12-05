using APPMVC.NET.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace APPMVC.NET.Components
{
    [ViewComponent]
    public class CategoryProductSidebar : ViewComponent
    {
        
        public class CategoryProductSidebarData
        {
            public List<CategoryProduct>? Categories { get; set; }
            public int Level { get; set; }
            public string? CategorySlug { get; set; }
        }
        public IViewComponentResult Invoke(CategoryProductSidebarData data)
        {
            return View(data);
        }
    }
}