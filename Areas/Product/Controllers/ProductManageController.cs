using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using APPMVC.NET.Data;
using APPMVC.NET.Models;
using APPMVC.NET.Models.Product;
using APPMVC.NET.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;
#nullable disable warnings

namespace APPMVC.NET.Areas.Product.Controllers {
    [Area("Product")]
    [Route("admin/productmanage/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class ProductManageController : Controller {
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _usermanager;

        private readonly ILogger<ProductManageController> _logger;

        public ProductManageController (AppDbContext context,
            UserManager<AppUser> usermanager,
            ILogger<ProductManageController> logger) {
            _context = context;
            _usermanager = usermanager;
            _logger = logger;
        }


        [TempData]
        public string StatusMessage { get; set; }

        public const int ITEMS_PER_PAGE = 5;
        // GET: Admin/Post
        public async Task<IActionResult> Index ([FromQuery(Name = "p")]int pageNumber) {

            var listPosts = _context.Products
                .Include (p => p.Author)
                .Include (p => p.ProductCategoryProducts)
                .ThenInclude (c => c.Category)
                .OrderByDescending (p => p.DateCreated);

            _logger.LogInformation (pageNumber.ToString ());

            // Lấy tổng số dòng dữ liệu
            var totalItems = await listPosts.CountAsync();
            ViewData["item"] = totalItems;
            // Tính số trang hiện thị (mỗi trang hiện thị ITEMS_PER_PAGE mục)
            int totalPages = (int)Math.Ceiling ((double) totalItems / ITEMS_PER_PAGE);

            if (pageNumber > totalPages)
                pageNumber = totalPages;
            if (pageNumber == 0)
                pageNumber = 1;

            ViewBag.postIndex = ITEMS_PER_PAGE * (pageNumber - 1);

            var posts = await listPosts
                .Skip(ITEMS_PER_PAGE * (pageNumber - 1))
                .Include (p => p.Author)
                .Include (p => p.ProductCategoryProducts)
                .Take(ITEMS_PER_PAGE)
                .ToListAsync ();

            // return View (await listPosts.ToListAsync());
            ViewData["pageNumber"] = pageNumber;
            ViewData["totalPages"] = totalPages;

            return View (posts);
        }

        // GET: Admin/Post/Details/5
        public async Task<IActionResult> Details (int? id) {
            if (id == null) {
                return NotFound ();
            }

            var post = await _context.Products
                .Include (p => p.Author)
                .FirstOrDefaultAsync (m => m.ProductID == id);
            if (post == null) {
                return NotFound ();
            }

            return View (post);
        }

        [BindProperty]
        public int[] selectedCategories { set; get; }

        // GET: Admin/Post/Create
        public async Task<IActionResult> CreateAsync () {
            // Thông tin về User tạo Post
            var user = await _usermanager.GetUserAsync (User);
            ViewData["userpost"] = $"{user.UserName}";

            // Danh mục chọn để đăng bài Post, tạo MultiSelectList
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList (categories, "Id", "Title");
            return View ();
        }

        // POST: Admin/Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create ([Bind("Title,Description,Slug,Content,Published,Price")] ProductModel product) {

            var user = await _usermanager.GetUserAsync(User);
            ViewData["userpost"] = $"{user.UserName}";

            // Phát sinh Slug theo Title
            if (product.Slug == null) {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
                ModelState.SetModelValue("Slug", new ValueProviderResult(product.Slug));

                // Thiết lập và kiểm tra lại Model
                ModelState.Clear();
                TryValidateModel(product);
            }

            if (selectedCategories.Length == 0) {
                ModelState.AddModelError (string.Empty, "Phải ít nhất một chuyên mục");
            }

            bool SlugExisted = await _context.Products.Where(p => p.Slug == product.Slug).AnyAsync();
            if (SlugExisted) 
            {
                ModelState.AddModelError(nameof (product.Slug), "Slug đã có trong Database");
            }

            if (ModelState.IsValid) {
                product.DateCreated = product.DateUpdated = DateTime.Now;
                product.AuthorId = user.Id;
                _context.Add(product);

                // Chèn thông tin về ProductCategoryProduct của bài Post
                if (selectedCategories != null)
                {
                    foreach (var selectedCategory in selectedCategories) 
                    {
                    _context.Add(new ProductCategoryProduct () { Product = product, CategoryID = selectedCategory });
                    }
                }
                await _context.SaveChangesAsync ();

                StatusMessage = "Vừa tạo bài viết mới";

                return RedirectToAction (nameof (Index));
            }

            var categories = await _context.CategoryProducts.ToListAsync ();
            ViewData["categories"] = new MultiSelectList (categories, "Id", "Title", selectedCategories);
            
            return View (product);
        }

        // GET: Admin/Post/Edit/5
        public async Task<IActionResult> Edit (int? id) 
        {
            if (id == null) 
            {
                return NotFound ();
            }

            // var post = await _context.Products.FindAsync (id);
            var post = await _context.Products.Where (p => p.ProductID == id)
                .Include (p => p.Author)
                .Include (p => p.ProductCategoryProducts)
                .ThenInclude (c => c.Category).FirstOrDefaultAsync ();
            if (post == null) {
                return NotFound ();
            }

            // ViewData["userpost"] = $"{post.Author.UserName} {post.Author.FullName}";
            ViewData["datecreate"] = post.DateCreated.ToShortDateString ();

            // Danh mục chọn
            var selectedCates = post.ProductCategoryProducts.Select (c => c.CategoryID).ToArray ();
            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList (categories, "Id", "Title", selectedCates);

            return View (post);
        }

        // POST: Admin/Post/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (int id, [Bind("ProductID,Title,Description,Slug,Content,Published,Price")] ProductModel product) {

            // if (id != post.ProductID) {
            //     return NotFound ();
            // }

            // Phát sinh Slug theo Title
            if (product.Slug == null) 
            {
                product.Slug = AppUtilities.GenerateSlug(product.Title);
                ModelState.SetModelValue("Slug", new ValueProviderResult (product.Slug));
                
                // Thiết lập và kiểm tra lại Model
                ModelState.Clear();
                TryValidateModel(product);
            }

            if (selectedCategories.Length == 0) {
                ModelState.AddModelError (string.Empty, "Phải ít nhất một chuyên mục");
            }

            if (await _context.Products.AnyAsync(p => p.Slug == product.Slug && id == product.ProductID)) 
            {
                ModelState.AddModelError(nameof (product.Slug), "Slug đã có trong Database");
            }

            if (ModelState.IsValid) {
                try {
                    // Lấy nội dung từ DB
                var productUpdate = await _context.Products.Include(p => p.ProductCategoryProducts)
                .FirstOrDefaultAsync(p => p.ProductID == id);
                if (productUpdate == null) {
                    return NotFound ();
                }

                // Cập nhật nội dung mới
                productUpdate.Title = product.Title;
                productUpdate.Description = product.Description;
                productUpdate.Content = product.Content;
                productUpdate.Slug = product.Slug;
                productUpdate.Published = product.Published;
                productUpdate.DateUpdated = DateTime.Now;
                productUpdate.Price = product.Price;

                // Các danh mục không có trong selectedCategories
                var listcateremove = productUpdate.ProductCategoryProducts
                                               .Where(p => !selectedCategories.Contains(p.CategoryID))
                                               .ToList();
                listcateremove.ForEach(c => productUpdate.ProductCategoryProducts.Remove(c));

                // Các ID category chưa có trong postUpdate.ProductCategoryProducts
                var listCateAdd = selectedCategories
                                    .Where(
                                        id => !productUpdate.ProductCategoryProducts.Where(c => c.CategoryID == id).Any()
                                    ).ToList();

                listCateAdd.ForEach(id => {
                    productUpdate.ProductCategoryProducts.Add(new ProductCategoryProduct() {
                        ProductID = productUpdate.ProductID,
                        CategoryID = id
                    });
                });
         
                    _context.Update(productUpdate);

                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                    if (!PostExists (product.ProductID)) {
                        return NotFound ();
                    } else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.CategoryProducts.ToListAsync();
            ViewData["categories"] = new MultiSelectList (categories, "Id", "Title", selectedCategories);
            return View (product);
        }

        // GET: Admin/Post/Delete/5
        public async Task<IActionResult> Delete (int? id) 
        {
            if (id == null) {
                return NotFound();
            }

            var post = await _context.Products
                .Include (p => p.Author)
                .FirstOrDefaultAsync (m => m.ProductID == id);

            if (post == null) {
                return NotFound();
            }

            return View(post);
        }

        // POST: Admin/Post/Delete/5
        [HttpPost, ActionName ("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed (int id) 
        {
            var post = await _context.Products.FindAsync (id);
            _context.Products.Remove (post);
            await _context.SaveChangesAsync ();

            StatusMessage = "Bạn vừa xóa bài viết: " + post.Title;

            return RedirectToAction (nameof (Index));
        }

        private bool PostExists (int id) {
            return _context.Products.Any (e => e.ProductID == id);
        }

        public class UploadOneFile
        {
            [Required(ErrorMessage = "Phải chọn file upload")]
            [DataType(DataType.Upload)]
            [FileExtensions(Extensions = "png,jpg,jpeg,gif")]
            [Display(Name = "Chọn file upload")]
            public IFormFile FileUpload { get; set; }
        }

        [HttpGet]
        public IActionResult UploadPhoto(int id)
        {
            var product = _context.Products.Where(p => p.ProductID == id)
                                        .Include(p => p.Photos)
                                        .FirstOrDefault();
            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }
            ViewData["product"] = product;

            return View(new UploadOneFile());
        }

        [HttpPost, ActionName("UploadPhoto")]
        public async Task<IActionResult> UploadPhotoAsync(int id, [Bind("FileUpload")] UploadOneFile file)
        {
            var product = _context.Products.Where(p => p.ProductID == id)
                                        .Include(p => p.Photos)
                                        .FirstOrDefault();
            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }
            ViewData["product"] = product;

            if (file != null)
            {
                var firstFile = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                                + Path.GetExtension(file.FileUpload.FileName);
                var secondFile = Path.Combine("Uploads", "Products", firstFile);

                using var filestream = new FileStream(secondFile, FileMode.Create);
                await file.FileUpload.CopyToAsync(filestream);

                _context.Add(new ProductPhoto(){
                    ProductID = product.ProductID,
                    FileName = firstFile
                });

                await _context.SaveChangesAsync();

            }

            return View(file);
        }

        [HttpPost]
        public IActionResult ListPhotos(int id)
        {
            var product = _context.Products.Where(p => p.ProductID == id)
                                        .Include(p => p.Photos)
                                        .FirstOrDefault();
            if (product == null)
            {
                return Json(
                    new {
                        success = 0,
                        message = "Product not found"
                    }
                );
            }

            var listphotos = product.Photos.Select(photo => new {
                id = photo.Id,
                path = "/contents/Products/" + photo.FileName
            });

            return Json(
                new {
                    success = 1,
                    photos = listphotos,
                }
            );
        }

        [HttpPost]
        public IActionResult DeletePhoto(int id)
        {
            var photo = _context.ProductPhotos.Where(p => p.Id == id).FirstOrDefault();
            if (photo != null)
            {
                _context.Remove(photo);
                _context.SaveChanges();

                var fileName = "Uploads/Products/" + photo.FileName;
                System.IO.File.Delete(fileName);
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UploadPhotoApi(int id, [Bind("FileUpload")] UploadOneFile file)
        {
            var product = _context.Products.Where(p => p.ProductID == id)
                                        .Include(p => p.Photos)
                                        .FirstOrDefault();
            if (product == null)
            {
                return NotFound("Không có sản phẩm");
            }

            if (file != null)
            {
                var firstFile = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                                + Path.GetExtension(file.FileUpload.FileName);
                var secondFile = Path.Combine("Uploads", "Products", firstFile);

                using var filestream = new FileStream(secondFile, FileMode.Create);
                await file.FileUpload.CopyToAsync(filestream);

                _context.Add(new ProductPhoto(){
                    ProductID = product.ProductID,
                    FileName = firstFile
                });

                await _context.SaveChangesAsync();

            }

            return Ok();
        }
    }
}