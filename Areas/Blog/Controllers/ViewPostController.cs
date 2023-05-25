using App.Data;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Blog.Controllers
{
  [Area("Blog")]
  public class ViewPostController : Controller

  {
    private readonly AppDbContext _context;
    private readonly ILogger<ViewPostController> _logger;

    public ViewPostController(AppDbContext context, ILogger<ViewPostController> logger)
    {
      _context = context;
      _logger = logger;
    }

    [Route("/post/{categorySlug?}")]
    public IActionResult Index(string categorySlug, [FromQuery(Name = "p")] int currentPage, int pageSize)
    {
      var categories = GetCategories();

      Category? category = null;
      if (categorySlug != null)
      {

        category = _context.Categories
              .Where(c => c.Slug == categorySlug)
              .Include(c => c.CategoryChildren)
              .FirstOrDefault();

        if (category == null)
        {
          return NotFound("Could not found category");
        }
      }
      var posts = _context.Posts
                        .Include(c => c.Author)
                        .Include(c => c.PostCategories)
                        .ThenInclude(c => c.Category)
                        .AsQueryable();
      posts.OrderByDescending(c => c.CreatAt);
      if (category != null)
      {
        var ids = new List<int>();
        category.CategoryChildrenIds(null, ids);
        ids.Add(category.Id);
        posts = posts.Where(p => p.PostCategories.Where(pc => ids.Contains(pc.CategoryID)).Any());
      }
      var total = posts.Count();
      if (pageSize < 1) pageSize = 10;
      var countPages = (int)Math.Ceiling((double)total / pageSize);
      if (currentPage < 1)
        currentPage = 1;
      if (currentPage > countPages)
        currentPage = countPages;
      var pagingmodel = new PagingModel
      {
        countpages = countPages,
        currentpage = currentPage,
        generateUrl = pageNumber =>
        {
          var values = new
          {
            p = pageNumber,
            pageSize = pageSize
          };
          return Url.Action("Index", values) ?? string.Empty;
        }
      };
      var postsInPage = new List<Post>();
      if (posts.Count() > 0)
      {
        postsInPage = posts.Skip((currentPage - 1) * pageSize)
                                    .Take(pageSize)
                                    .ToList();
      }
      ViewBag.categorySlug = categorySlug;
      ViewBag.categories = categories;
      ViewBag.category = category;
      ViewBag.pagingmodel = pagingmodel;
      return View(postsInPage);
    }
    [Route("/post/{postSlug}.html")]
    public IActionResult Detail(string postSlug)
    {
      var categories = GetCategories();
      var post = _context.Posts.Include(p => p.PostCategories)
                                .Include(p => p.Author)
                                .Where(p => p.Slug == postSlug)
                                .FirstOrDefault();
      if (post == null) return NotFound();
      ViewBag.categories = categories;
      Category? category = post.PostCategories.FirstOrDefault()?.Category;
      var relatedPost = _context.Posts.Where(p => p.PostCategories.Any(c => c.Category.Id == category.Id)).ToList();
      ViewBag.relatedPost = relatedPost;
      return View(post);
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