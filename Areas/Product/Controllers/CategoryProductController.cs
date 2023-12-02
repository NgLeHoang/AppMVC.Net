using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using APPMVC.NET.Models;
using APPMVC.NET.Models.Product;
using APPMVC.NET.Data;

namespace APPMVC.NET.Areas.Admin.Product.Controllers
{
    [Area ("Product")]
    [Route("admin/categoryproduct/category/[action]/{id?}")]
    [Authorize(Roles = RoleName.Administrator)]
    public class CategoryProductController : Controller {
        private readonly AppDbContext _context;

        public CategoryProductController (AppDbContext context) {
            _context = context;
        }

        // GET: Admin/Category
        public IActionResult Index () {

            var items =  _context.CategoryProducts
                .Include (c => c.CategoryChildren)   // <-- Nạp các Category con
                .AsEnumerable()
                .Where (c => c.ParentCategory == null)
                .ToList();
           

            return View (items);

        }

        // GET: Admin/Category/Details/5
        public async Task<IActionResult> Details (int? id) {
            if (id == null) {
                return NotFound ();
            }

            var category = await _context.CategoryProducts
                .Include (c => c.ParentCategory)
                .FirstOrDefaultAsync (m => m.Id == id);
            if (category == null) {
                return NotFound ();
            }

            return View (category);
        }

        // GET: Admin/Category/Create
        public async Task<IActionResult> Create () {
            // ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Slug");
            var listcategory = await _context.CategoryProducts.ToListAsync ();
            listcategory.Insert(0, new CategoryProduct () {
                Title = "Không có danh mục cha",
                    Id = -1
            });
            ViewData["ParentCategoryId"] = new SelectList (await GetItemsSelectCategorie(), "Id", "Title", -1);
            return View ();
        }

        async Task<IEnumerable<CategoryProduct>> GetItemsSelectCategorie() {

            var items = await _context.CategoryProducts
                                .Include(c => c.CategoryChildren)
                                .Where(c => c.ParentCategory == null)
                                .ToListAsync();

            List<CategoryProduct> resultitems = new List<CategoryProduct>() {
                new CategoryProduct() {
                    Id = -1,
                    Title = "Không có danh mục cha"
                }
            };
            Action<List<CategoryProduct>, int> _ChangeTitleCategory = null;
            Action<List<CategoryProduct>, int> ChangeTitleCategory =  (items, level) => {
                string prefix = String.Concat(Enumerable.Repeat("—", level));
                foreach (var item in items) {
                    item.Title = prefix + " " + item.Title; 
                    resultitems.Add(item);
                    if ((item.CategoryChildren != null) && (item.CategoryChildren.Count > 0)) {
                        _ChangeTitleCategory(item.CategoryChildren.ToList(), level + 1);
                    }
                        
                }
                
            };

            _ChangeTitleCategory = ChangeTitleCategory;
            ChangeTitleCategory(items, 0);

            return resultitems;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind ("Id,ParentCategoryId,Title,Description,Slug")] CategoryProduct category) {
            if (ModelState.IsValid) {
                if (category.ParentCategoryId.Value == -1)
                    category.ParentCategoryId = null;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof (Index));
            }

            // ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Slug", category.ParentCategoryId);
            var listcategory = await _context.CategoryProducts.ToListAsync ();
            listcategory.Insert(0, new CategoryProduct () 
            {
                Title = "Không có danh mục cha",
                    Id = -1
            });
            ViewData["ParentCategoryId"] = new SelectList (await GetItemsSelectCategorie(), "Id", "Title", category.ParentCategoryId);
            return View (category);
        }

        // GET: Admin/Category/Edit/5
        public async Task<IActionResult> Edit (int? id) {
            if (id == null) {
                return NotFound ();
            }

            var category = await _context.CategoryProducts.FindAsync (id);
            if (category == null) {
                return NotFound ();
            }
            
            ViewData["ParentCategoryId"] = new SelectList (await GetItemsSelectCategorie(), "Id", "Title", category.ParentCategoryId);

            return View (category);
        }

        // POST: Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit (int id, [Bind ("Id,ParentCategoryId,Title,Description,Slug")] CategoryProduct category) {
            if (id != category.Id) {
                return NotFound ();
            }

            if (category.ParentCategoryId == category.Id)
            {
                ModelState.AddModelError(string.Empty, "Phải chọn danh mục cha khác");
            }

            if (ModelState.IsValid && category.ParentCategoryId != category.Id) {
                try {
                    if (category.ParentCategoryId == -1) {
                        category.ParentCategoryId = null;
                    }
                    _context.Update (category);
                    await _context.SaveChangesAsync ();
                } catch (DbUpdateConcurrencyException) {
                    if (!CategoryExists (category.Id)) {
                        return NotFound ();
                    } else {
                        throw;
                    }
                }
                return RedirectToAction (nameof (Index));
            }
            var listcategory = await _context.CategoryProducts.ToListAsync ();
            listcategory.Insert (0, new CategoryProduct () {
                Title = "Không có danh mục cha",
                    Id = -1
            });
            ViewData["ParentCategoryId"] = new SelectList (listcategory, "Id", "Title", category.ParentCategoryId);
            return View (category);
        }

        // GET: Admin/Category/Delete/5
        public async Task<IActionResult> Delete (int? id) {
            if (id == null) {
                return NotFound ();
            }

            var category = await _context.CategoryProducts
                .Include (c => c.ParentCategory)
                .FirstOrDefaultAsync (m => m.Id == id);
            if (category == null) {
                return NotFound ();
            }

            return View (category);
        }

        // POST: Admin/Category/Delete/5
        [HttpPost, ActionName ("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed (int id) {
            var category = await _context.CategoryProducts.FindAsync(id);
            _context.CategoryProducts.Remove(category);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof (Index));
        }

        private bool CategoryExists (int id) {
            return _context.CategoryProducts.Any (e => e.Id == id);
        }
    }
}