using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APPMVC.NET.Models;
using APPMVC.NET.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace APPMVC.NET.Areas.Admin.Blog.Controllers
{
    [Area("Blog")]
    public class ViewPostController : Controller
    {
        private readonly ILogger<ViewPostController> _logger;
        private readonly AppDbContext _context;
        public ViewPostController(ILogger<ViewPostController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Route("/post/{categoryslug?}")]
        public async Task<IActionResult> Index(string categoryslug, [FromQuery(Name = "p")] int currentPage, int pagesize)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            ViewBag.CategorySlug = categoryslug;

            Category? category = null;
            if (!string.IsNullOrEmpty(categoryslug))
            {
                category = _context.Categories.Where(c => c.Slug == categoryslug)
                                   .Include(c => c.CategoryChildren)
                                   .FirstOrDefault();
                if (category == null)
                {
                    return NotFound("Không thấy category");
                }
            }

            var posts = _context.Posts
                                .Include(p => p.Author)
                                .Include(p => p.PostCategories)
                                .ThenInclude(p => p.Category)
                                .AsQueryable();

            posts.OrderByDescending(p => p.DateUpdated);
            
            if (category != null)
            {
                var ids = new List<int>();
                category.ChildCategoryIDs(null, ids);
                ids.Add(category.Id);

                posts = posts.Where(p => p.PostCategories.Any(pc => ids.Contains(pc.CategoryID)));
            }

            int totalPosts = posts.Count();
            if (pagesize <= 0) pagesize = 5;
            int countPages = (int)Math.Ceiling((double)totalPosts / pagesize);

            if (currentPage > countPages) currentPage = countPages;
            if (currentPage < 1) currentPage = 1;

            var pagingModel = new PagingModel()
            {
                countpages = countPages,
                currentpage = currentPage,
                generateUrl = (pageNumber) => Url.Action("Index", new {
                    p = pageNumber,
                    pagesize
                })
            };

            var postInPage = posts.Skip((currentPage - 1) * pagesize)
                                  .Take(pagesize)
                                  .OrderByDescending(p => p.DateUpdated);

            ViewBag.pagingModel = pagingModel;
            ViewBag.totalPosts = totalPosts;

            ViewBag.category = category;
            return View(await postInPage.ToListAsync());
        }

        [Route("/post/{postslug}.html")]
        public IActionResult Details(string postslug)
        {
            var categories = GetCategories();
            ViewBag.categories = categories;
            
            var post = _context.Posts.Where(p => p.Slug == postslug)
                                    .Include(p => p.Author)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(pc => pc.Category)
                                    .FirstOrDefault();
            if (post == null)
            {
                return NotFound("Không thấy bài viết");
            }

            Category? category = post.PostCategories?.FirstOrDefault()?.Category;
            ViewBag.category = category;

            var otherPosts = _context.Posts.Where(p => p.PostCategories.Any(c => c.Category.Id == category.Id))
                                            .Where(p => p.PostID != post.PostID)
                                            .OrderByDescending(p => p.DateUpdated)
                                            .Take(5);
            ViewBag.otherPosts = otherPosts;
            return View(post);
            //return Content("FIX BUG");
        }

        private List<Category> GetCategories() 
        {
            var categories = _context.Categories
                            .Include(c => c.CategoryChildren)
                            .AsEnumerable()
                            .Where(c => c.ParentCategory == null)
                            .ToList();

            return categories;
        }
    }
}