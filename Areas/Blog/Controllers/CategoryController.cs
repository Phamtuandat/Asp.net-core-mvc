
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Models.Blog;
using Microsoft.AspNetCore.Authorization;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/Category/[action]/{id?}")]
    [Authorize(Roles = RoleNames.Administrator)]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(AppDbContext context, ILogger<CategoryController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [TempData]
        public string Message { get; set; } = string.Empty;
        // GET: Blog
        public async Task<IActionResult> Index()
        {
            var appDbContext = (from c in _context.Categories select c)
                                    .Include(c => c.ParentCategory)
                                    .Include(c => c.CategoryChildren);
            var categoryList = (await appDbContext.ToListAsync())
                                    .Where(c => c.ParentCategory == null).ToList();
            return View(categoryList);
        }

        // GET: Blog/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        private void CreateSelectItem(List<Category> source, List<CategorySelecItem> des, int level)
        {
            foreach (Category category in source)
            {
                var prefix = string.Concat(Enumerable.Repeat("---", level));
                des.Add(new CategorySelecItem()
                {
                    Id = category.Id,
                    Title = prefix + " " + category.Title,
                    level = level,

                });
                if (category.CategoryChildren?.Count > 0)
                {
                    CreateSelectItem(category.CategoryChildren.ToList(), des, level + 1);
                }
            }
        }
        // GET: Blog/Create
        public async Task<IActionResult> Create()
        {
            var appDbContext = (from c in _context.Categories select c)
                                    .Include(c => c.ParentCategory)
                                    .Include(c => c.CategoryChildren);
            var categoryList = (await appDbContext.ToListAsync())
                                    .Where(c => c.ParentCategory == null).ToList();
            categoryList.Insert(0, new Category()
            {
                Title = "None",
                Id = -1,

            });
            var items = new List<CategorySelecItem>();
            CreateSelectItem(categoryList, items, 0);
            ViewData["ParentCategoryId"] = new SelectList(items, "Id", "Title");
            return View();
        }

        // POST: Blog/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Slug,ParentCategoryId")] Category category)
        {
            if (ModelState.IsValid)
            {
                if (category.ParentCategoryId == -1) category.ParentCategory = null;
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Slug", category.ParentCategoryId);
            return View(category);
        }

        // GET: Blog/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            var appDbContext = (from c in _context.Categories select c)
                                    .Include(c => c.ParentCategory)
                                    .Include(c => c.CategoryChildren);
            var categoryList = (await appDbContext.ToListAsync())
                                    .Where(c => c.ParentCategory == null).ToList();

            categoryList.Insert(0, new Category()
            {
                Id = -1,
                Title = "None"
            });
            var items = new List<CategorySelecItem>();
            CreateSelectItem(categoryList, items, 0);
            var categoryEditItem = items.FirstOrDefault(c => c.Id == id);
            if (categoryEditItem != null)
            {
                var selectList = items.Where(c => c.Id != id && c.level < categoryEditItem.level).ToList();
                selectList.Remove(categoryEditItem);
                ViewData["ParentCategoryId"] = new SelectList(selectList, "Id", "Title", category.ParentCategoryId);
            }
            return View(category);
        }
        // check parent category is valid.
        private bool canUpdateCategory(List<Category> categories, Category category)
        {
            var parentCategoryId = category.ParentCategoryId;
            if (categories.Count() == 0 || parentCategoryId == -1)
            {
                return true;
            }
            else if (categories.Count() > 0)
            {
                foreach (Category c in categories)
                {
                    if (c.Id == parentCategoryId) return false;
                    if (c.CategoryChildren == null) continue;
                    return canUpdateCategory(c.CategoryChildren.ToList(), category);
                }
            }
            return false;
        }

        // POST: Blog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Slug,ParentCategoryId")] Category category)
        {

            var appDbContext = (from c in _context.Categories select c)
                                                .Include(c => c.ParentCategory)
                                                .Include(c => c.CategoryChildren);
            var editCategory = (await appDbContext.ToListAsync())
                                    .FirstOrDefault(c => c.Id == id);
            if (editCategory == null) return NotFound();

            var isValid = canUpdateCategory(editCategory.CategoryChildren.ToList(), category);
            if (!isValid)
            {
                Message = "Can't update with parent category is already a children of this category";
            }
            if (ModelState.IsValid && isValid)
            {
                try
                {
                    if (category.ParentCategoryId == -1) category.ParentCategoryId = null;
                    if (category.ParentCategoryId == category.Id)
                    {
                        Message = "Could not update category";
                        return RedirectToAction(nameof(Index));
                    };
                    editCategory.Title = category.Title;
                    editCategory.Description = category.Description;
                    editCategory.Slug = category.Slug;
                    editCategory.ParentCategoryId = category.Id;
                    editCategory.ParentCategoryId = category.ParentCategoryId;
                    _context.Update(editCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var items = new List<CategorySelecItem>();
            CreateSelectItem(appDbContext.Where(c => c.CategoryChildren == null).ToList(), items, 0);
            var categoryEditItem = items.FirstOrDefault(c => c.Id == id);
            if (categoryEditItem != null)
            {
                var selectList = items.Where(c => c.Id != id && c.level < categoryEditItem.level).ToList();
                selectList.Remove(categoryEditItem);
                ViewData["ParentCategoryId"] = new SelectList(selectList, "Id", "Title", category.ParentCategoryId);
            }
            return RedirectToAction(nameof(Edit));
        }

        // GET: Blog/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Blog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'AppDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.Include(c => c.CategoryChildren)
                                                    .Include(category => category.ParentCategory)
                                                    .FirstOrDefaultAsync(c => c.Id == id);
            if (category != null)
            {
                if (category.CategoryChildren?.Count > 0)
                {
                    foreach (var item in category.CategoryChildren)
                    {
                        item.ParentCategory = category.ParentCategory;
                    }
                }
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
