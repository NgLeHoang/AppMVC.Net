using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APPMVC.NET.Areas.Blog.Models;
using APPMVC.NET.Data;
using APPMVC.NET.Models;
using APPMVC.NET.Models.Blog;
using APPMVC.NET.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace APPMVC.NET.Areas.Blog.Controllers {
    [Area("Blog")]
    [Route("admin/blog/post/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class PostController : Controller {
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _usermanager;

        private readonly ILogger<PostController> _logger;

        public PostController (AppDbContext context,
            UserManager<AppUser> usermanager,
            ILogger<PostController> logger) {
            _context = context;
            _usermanager = usermanager;
            _logger = logger;
        }


        [TempData]
        public string StatusMessage { get; set; }

        public const int ITEMS_PER_PAGE = 5;
        // GET: Admin/Post
        public async Task<IActionResult> Index ([FromQuery(Name = "p")]int pageNumber) {

            var listPosts = _context.Posts
                .Include (p => p.Author)
                .Include (p => p.PostCategories)
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
                .Include (p => p.PostCategories)
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

            var post = await _context.Posts
                .Include (p => p.Author)
                .FirstOrDefaultAsync (m => m.PostID == id);
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
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList (categories, "Id", "Title");
            return View ();
        }

        // POST: Admin/Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create ([Bind("Title,Description,Slug,Content,Published")] CreatePostModel post) {

            var user = await _usermanager.GetUserAsync(User);
            ViewData["userpost"] = $"{user.UserName}";

            // Phát sinh Slug theo Title
            if (post.Slug == null) {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
                ModelState.SetModelValue("Slug", new ValueProviderResult(post.Slug));

                // Thiết lập và kiểm tra lại Model
                ModelState.Clear();
                TryValidateModel(post);
            }

            if (selectedCategories.Length == 0) {
                ModelState.AddModelError (string.Empty, "Phải ít nhất một chuyên mục");
            }

            bool SlugExisted = await _context.Posts.Where(p => p.Slug == post.Slug).AnyAsync();
            if (SlugExisted) 
            {
                ModelState.AddModelError(nameof (post.Slug), "Slug đã có trong Database");
            }

            if (ModelState.IsValid) {
                post.DateCreated = post.DateUpdated = DateTime.Now;
                post.AuthorId = user.Id;
                _context.Add(post);

                // Chèn thông tin về PostCategory của bài Post
                if (selectedCategories != null)
                {
                    foreach (var selectedCategory in selectedCategories) 
                    {
                    _context.Add(new PostCategory () { Post = post, CategoryID = selectedCategory });
                    }
                }
                await _context.SaveChangesAsync ();

                StatusMessage = "Vừa tạo bài viết mới";

                return RedirectToAction (nameof (Index));
            }

            var categories = await _context.Categories.ToListAsync ();
            ViewData["categories"] = new MultiSelectList (categories, "Id", "Title", selectedCategories);
            
            return View (post);
        }

        // GET: Admin/Post/Edit/5
        public async Task<IActionResult> Edit (int? id) 
        {
            if (id == null) 
            {
                return NotFound ();
            }

            // var post = await _context.Posts.FindAsync (id);
            var post = await _context.Posts.Where (p => p.PostID == id)
                .Include (p => p.Author)
                .Include (p => p.PostCategories)
                .ThenInclude (c => c.Category).FirstOrDefaultAsync ();
            if (post == null) {
                return NotFound ();
            }

            // ViewData["userpost"] = $"{post.Author.UserName} {post.Author.FullName}";
            ViewData["datecreate"] = post.DateCreated.ToShortDateString ();

            // Danh mục chọn
            var selectedCates = post.PostCategories.Select (c => c.CategoryID).ToArray ();
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList (categories, "Id", "Title", selectedCates);

            return View (post);
        }

        // POST: Admin/Post/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (int id, [Bind("PostId,Title,Description,Slug,Content,Published")] CreatePostModel post) {

            // if (id != post.PostID) {
            //     return NotFound ();
            // }

            // Phát sinh Slug theo Title
            if (post.Slug == null) 
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
                ModelState.SetModelValue("Slug", new ValueProviderResult (post.Slug));
                
                // Thiết lập và kiểm tra lại Model
                ModelState.Clear();
                TryValidateModel(post);
            }

            if (selectedCategories.Length == 0) {
                ModelState.AddModelError (String.Empty, "Phải ít nhất một chuyên mục");
            }

            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug && id == post.PostID)) 
            {
                ModelState.AddModelError(nameof (post.Slug), "Slug đã có trong Database");
            }

            if (ModelState.IsValid) {
                try {
                    // Lấy nội dung từ DB
                var postUpdate = await _context.Posts.Include(p => p.PostCategories)
                .FirstOrDefaultAsync(p => p.PostID == id);
                if (postUpdate == null) {
                    return NotFound ();
                }

                // Cập nhật nội dung mới
                postUpdate.Title = post.Title;
                postUpdate.Description = post.Description;
                postUpdate.Content = post.Content;
                postUpdate.Slug = post.Slug;
                postUpdate.Published = post.Published;
                postUpdate.DateUpdated = DateTime.Now;

                // Các danh mục không có trong selectedCategories
                var listcateremove = postUpdate.PostCategories
                                               .Where(p => !selectedCategories.Contains(p.CategoryID))
                                               .ToList();
                listcateremove.ForEach(c => postUpdate.PostCategories.Remove(c));

                // Các ID category chưa có trong postUpdate.PostCategories
                var listCateAdd = selectedCategories
                                    .Where(
                                        id => !postUpdate.PostCategories.Where(c => c.CategoryID == id).Any()
                                    ).ToList();

                listCateAdd.ForEach(id => {
                    postUpdate.PostCategories.Add(new PostCategory() {
                        PostID = postUpdate.PostID,
                        CategoryID = id
                    });
                });
         
                    _context.Update(postUpdate);

                    await _context.SaveChangesAsync();
                } catch (DbUpdateConcurrencyException) {
                    if (!PostExists (post.PostID)) {
                        return NotFound ();
                    } else {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList (categories, "Id", "Title", selectedCategories);
            return View (post);
        }

        // GET: Admin/Post/Delete/5
        public async Task<IActionResult> Delete (int? id) 
        {
            if (id == null) {
                return NotFound();
            }

            var post = await _context.Posts
                .Include (p => p.Author)
                .FirstOrDefaultAsync (m => m.PostID == id);

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
            var post = await _context.Posts.FindAsync (id);
            _context.Posts.Remove (post);
            await _context.SaveChangesAsync ();

            StatusMessage = "Bạn vừa xóa bài viết: " + post.Title;

            return RedirectToAction (nameof (Index));
        }

        private bool PostExists (int id) {
            return _context.Posts.Any (e => e.PostID == id);
        }
    }
}