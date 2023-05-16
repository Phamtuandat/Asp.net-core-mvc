
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Models.Blog;
using Microsoft.AspNetCore.Authorization;

namespace MVC.Areas.Blog.Controllers
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
        private void CreateSelectItem(List<Category> source, List<Category> des, int level)
        {
            foreach (Category category in source)
            {
                var prefix = string.Concat(Enumerable.Repeat("---", level));
                des.Add(new Category()
                {
                    Id = category.Id,
                    Title = prefix + " " + category.Title,
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
            var items = new List<Category>();
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
            var items = new List<Category>();
            items.Insert(0, new Category()
            {
                Id = -1,
                Title = "None"
            });
            CreateSelectItem(categoryList, items, 0);
            var selectList = items.Where(c => c.Id != id);
            ViewData["ParentCategoryId"] = new SelectList(selectList, "Id", "Title", category.ParentCategoryId);
            return View(category);
        }

        // POST: Blog/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Slug,ParentCategoryId")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (category.ParentCategoryId == -1) category.ParentCategoryId = null;
                    if (category.ParentCategoryId == category.Id)
                    {
                        Message = "Could not update category";
                        return RedirectToAction(nameof(Index));
                    };
                    _context.Update(category);
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

            ViewData["ParentCategoryId"] = new SelectList(_context.Categories, "Id", "Title", category.ParentCategoryId);
            return View(category);
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
