using APPMVC.NET.Models.Product;

namespace APPMVC.NET.Areas.Product.Models
{
    public class CartItem
    {
        public int Quantity { get; set; }
        public ProductModel? Product { get; set; }
    }
}