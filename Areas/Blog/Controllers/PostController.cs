using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Data;
using App.Models;
using App.Areas.Blog.Models;
using Microsoft.AspNetCore.Identity;
using App.Utilities;
using AutoMapper;

namespace App.Areas.Blog.Controllers
{
    [Area("Blog")]
    [Route("admin/blog/Post/[action]/{id?}")]
    public class PostController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;

        public PostController(AppDbContext context, UserManager<AppUser> userManager, IMapper mapper)
        {
            _context = context;
            _userManager = userManager;
            _mapper = mapper;
        }
        [TempData]
        public string Message { get; set; } = string.Empty;


        // GET: Post
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pageSize = 10)
        {
            var posts = _context.Posts.Include(p => p.Author).OrderByDescending(p => p.CreatAt);

            var total = posts.Count();
            var countPages = (int)Math.Ceiling((double)total / 10);
            if (currentPage < 1)
                currentPage = 1;
            if (currentPage > countPages)
                currentPage = countPages;

            var pagingModel = new PagingModel
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
            ViewBag.PagingModel = pagingModel;
            ViewBag.totalPosts = total;
            ViewBag.postIndex = (currentPage - 1) * pageSize;
            var postsInPage = await posts.Skip((currentPage - 1) * pageSize)
                                    .Take(pageSize)
                                    .Include(p => p.PostCategories)
                                    .ThenInclude(pc => pc.Category)
                                    .ToListAsync();

            return View(postsInPage);
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["MultiSelectList"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Published,Slug,Content,CategoryIDs")] CreatePostModels post)
        {

            if (post.Slug == null)
            {
                post.Slug = AppUtilities.GenerateSlug(post.Title);
            }
            if (await _context.Posts.AnyAsync(p => p.Slug == post.Slug))
            {
                ModelState.AddModelError("Slug", "Slug is already existed!");
                return View(post);
            }
            if (post.CategoryIDs == null) post.CategoryIDs = new int[] { };
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(this.User);
                if (user == null) return View(post);
                var newPost = _mapper.Map<CreatePostModels, Post>(post);
                newPost.AuthorId = user.Id;
                newPost.CreatAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                newPost.EditAt = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                string slug = newPost.Slug;
                _context.Posts.Add(newPost);


                if (post?.CategoryIDs != null)
                {
                    foreach (var item in post.CategoryIDs)
                    {
                        _context.Add(new PostCategory()
                        {
                            Post = newPost,
                            CategoryID = item
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            var editPost = _mapper.Map<Post, EditPostModels>(post);
            editPost.CategoryIDs = post.PostCategories.Select(pc => pc.CategoryID).ToArray();
            var categories = await _context.Categories.ToListAsync();
            ViewData["MultiSelectList"] = new MultiSelectList(categories, "Id", "Title");
            ViewData["AuthorId"] = new SelectList(_context.AppUsers, "Id", "UserName", post.AuthorId);
            return View(editPost);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Published,Slug,Content,CategoryIDs")] EditPostModels post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var postUpdate = await _context.Posts.Include(p => p.PostCategories).FirstOrDefaultAsync(p => p.PostId == id);
                    if (postUpdate == null) return NotFound();
                    postUpdate.Content = post.Content;
                    postUpdate.Description = post.Description;
                    postUpdate.Published = post.Published;
                    postUpdate.Slug = post.Slug == null ? AppUtilities.GenerateSlug(post.Title) : post.Slug;
                    postUpdate.Title = post.Title;

                    if (post.CategoryIDs == null) post.CategoryIDs = new int[] { };

                    var oldCate = postUpdate.PostCategories.Select(c => c.CategoryID).ToArray();
                    var newCate = post.CategoryIDs;

                    var removeCate = from postcate in postUpdate.PostCategories
                                     where (!newCate.Contains(postcate.CategoryID))
                                     select postcate;
                    _context.PostCategories.RemoveRange(removeCate);

                    var addCate = from categoryId in newCate
                                  where !oldCate.Contains(categoryId)
                                  select categoryId;
                    foreach (var item in addCate)
                    {
                        _context.PostCategories.Add(new PostCategory()
                        {
                            CategoryID = item,
                            PostID = id
                        });
                    }
                    _context.Posts.Update(postUpdate);
                    await _context.SaveChangesAsync();
                    Message = "Update successfully!";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
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
            return RedirectToAction(nameof(Index));
        }

        // GET: Post/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'AppDbContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return (_context.Posts?.Any(e => e.PostId == id)).GetValueOrDefault();
        }
    }
}
