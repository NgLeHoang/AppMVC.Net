using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APPMVC.NET.Areas.Product.Models;
using APPMVC.NET.Models;
using APPMVC.NET.Models.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APPMVC.NET.Areas.Admin.Blog.Controllers
{
    [Area("Product")]
    public class ViewProductController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;
        private readonly CartService _cartService;
        public ViewProductController(ILogger<ViewPostController> logger, AppDbContext context, CartService cartService)
        {
            _logger = logger;
            _context = context;
            _cartService = cartService;
        }

        [Route("/product/{categoryslug?}")]
        public async Task<IActionResult> Index(string categoryslug, [FromQuery(Name = "p")] int currentPage, int pagesize)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.CategorySlug = categoryslug;

            CategoryProduct? category = null;
            if (!string.IsNullOrEmpty(categoryslug))
            {
                category = _context.CategoryProducts.Where(c => c.Slug == categoryslug)
                                   .Include(c => c.CategoryChildren)
                                   .FirstOrDefault();
                if (category == null)
                {
                    return NotFound("Không thấy category");
                }
            }

            var products = _context.Products
                                .Include(p => p.Author)
                                .Include(p => p.Photos)
                                .Include(p => p.ProductCategoryProducts)
                                .ThenInclude(p => p.Category)
                                .AsQueryable();

            products = products.OrderByDescending(p => p.DateUpdated);

            if (category != null)
            {
                var ids = new List<int>();
                category.ChildCategoryIDs(null, ids);
                ids.Add(category.Id);

                products = products.Where(p => p.ProductCategoryProducts.Any(pc => ids.Contains(pc.CategoryID)));
            }

            int totalProducts = products.Count();
            if (pagesize <= 0) pagesize = 6;
            int countPages = (int)Math.Ceiling((double)totalProducts / pagesize);

            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new
                {
                    p = pageNumber,
                    pagesize
                })
            };

            var productInPage = products.Skip((currentPage - 1) * pagesize)
                                  .Take(pagesize)
                                  .OrderByDescending(p => p.DateUpdated);

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalProducts;

            ViewBag.category = category;
            return View(await productInPage.ToListAsync());
        }

        [Route("/product/{productslug}.html")]
        public IActionResult Details(string productslug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;

            var product = _context.Products.Where(p => p.Slug == productslug)
                                    .Include(p => p.Author)
                                    .Include(p => p.Photos)
                                    .Include(p => p.ProductCategoryProducts)
                                    .ThenInclude(pc => pc.Category)
                                    .FirstOrDefault();
            if (product == null)
            {
                return NotFound("Không thấy bài viết");
            }

            CategoryProduct? category = product.ProductCategoryProducts?.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherProducts = _context.Products.Where(p => p.ProductCategoryProducts.Any(c => c.Category.Id == category.Id))
                                            .Where(p => p.ProductID != product.ProductID)
                                            .OrderByDescending(p => p.DateUpdated)
                                            .Take(5);
            ViewBag.otherPosts = otherProducts;
            return View(product);
            //return Content("FIX BUG");
        }

        private List<CategoryProduct> GetCategories()
        {
            var categories = _context.CategoryProducts
                            .Include(c => c.CategoryChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            return categories;
        }

        [Route("addcart/{productid:int}", Name = "addcart")]
        public IActionResult AddToCart([FromRoute] int productid)
        {

            var product = _context.Products
                .Where(p => p.ProductID == productid)
                .FirstOrDefault();
            if (product == null)
                return NotFound("Không có sản phẩm");

            // Xử lý đưa vào Cart ...
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.Product?.ProductID == productid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cartitem.Quantity++;
            }
            else
            {
                //  Thêm mới
                cart.Add(new CartItem() { Quantity = 1, Product = product });
            }

            // Lưu cart vào Session
            _cartService.SaveCartSession(cart);
            // Chuyển đến trang hiện thị Cart
            return RedirectToAction(nameof(Cart));
        }
        [Route("/cart", Name = "cart")]
        public IActionResult Cart()
        {
            return View(_cartService.GetCartItems());
        }

        [Route("/removecart/{productid:int}", Name = "removecart")]
        public IActionResult RemoveCart([FromRoute] int productid)
        {
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.Product.ProductID == productid);
            if (cartitem != null)
            {
                cart.Remove(cartitem);
            }

            _cartService.SaveCartSession(cart);
            return RedirectToAction(nameof(Cart));
        }

        [Route("/updatecart", Name = "updatecart")]
        [HttpPost]
        public IActionResult UpdateCart([FromForm] int productid, [FromForm] int quantity)
        {
            // Cập nhật Cart thay đổi số lượng quantity ...
            var cart = _cartService.GetCartItems();
            var cartitem = cart.Find(p => p.Product.ProductID == productid);
            if (cartitem != null)
            {
                // Đã tồn tại, tăng thêm 1
                cartitem.Quantity = quantity;
            }
            _cartService.SaveCartSession(cart);
            // Trả về mã thành công (không có nội dung gì - chỉ để Ajax gọi)
            return Ok();
        }

        [Route("/checkout")]
        public IActionResult Checkout() 
        {
            var cart = _cartService.GetCartItems();
            // Process get information

            _cartService.ClearCart();

            return Content("Đã gửi đơn hàng");
        }
    }
}