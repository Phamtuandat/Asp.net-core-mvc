using App.Data;
using App.Models;
using App.Models.Blog;
using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Area("Database")]
public class DatabaseController : Controller
{
      private readonly UserManager<AppUser> _userManager;
      private readonly RoleManager<IdentityRole> _roleManager;
      private readonly AppDbContext _context;
      private readonly ILogger<DatabaseController> _logger;

      public DatabaseController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, ILogger<DatabaseController> logger)
      {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
      }

      public ActionResult Index()
      {
            return View();
      }
      [HttpGet]
      public ActionResult DeleteDb()
      {
            return View();
      }
      [TempData]
      public string Message { get; set; } = string.Empty;
      [HttpPost]
      public async Task<ActionResult> DeleteDbAsync()
      {
            var success = await _context.Database.EnsureDeletedAsync();
            Message = success ? "Successfully delete database" : "Something went wrong";
            return RedirectToAction(nameof(Index));
      }
      [HttpPost]
      public async Task<ActionResult> Migration()
      {
            await _context.Database.MigrateAsync();
            Message = "Update database successfully!";
            return RedirectToAction(nameof(Index));
      }
      public async Task<ActionResult> SeedData()
      {
            var roleNames = typeof(RoleNames).GetFields().ToList();
            foreach (var item in roleNames)
            {
                  var roleName = item.GetRawConstantValue() as string;
                  if (roleName != null)
                  {
                        var role = await _roleManager.FindByNameAsync(roleName);
                        if (role == null)
                        {
                              role = new IdentityRole(roleName);
                              await _roleManager.CreateAsync(role);
                        }
                  }
            }
            var userAdmin = await _userManager.FindByEmailAsync("admin@admin.com");
            if (userAdmin == null)
            {
                  try
                  {
                        userAdmin = new AppUser()
                        {
                              UserName = "admin",
                              Email = "admin@admin.com",
                              EmailConfirmed = true
                        };
                        await _userManager.CreateAsync(userAdmin, "Phamdat11a1.");
                        await _userManager.AddToRoleAsync(userAdmin, RoleNames.Administrator);
                  }
                  catch (Exception ex)
                  {

                        throw new DbUpdateException(ex.Message);
                  }
            }
            await SeedPostCategory(userAdmin);
            return RedirectToAction(nameof(Index));
      }

      private async Task SeedPostCategory(AppUser user)
      {

            _context.Categories.RemoveRange(_context.Categories.Where(c => c.Description.Contains("[fakeData]")));
            _context.Posts.RemoveRange(_context.Posts.Where(p => p.Description.Contains("[fakeData]")));

            var fakerCategory = new Faker<Category>();

            int cm = 1;
            fakerCategory.RuleFor(c => c.Title, fk => $"CM{cm++}" + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakerCategory.RuleFor(c => c.Description, fk => fk.Lorem.Sentence(5) + "[fakeData]");
            fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

            var cate = fakerCategory.Generate();
            var cate11 = fakerCategory.Generate();
            var cate12 = fakerCategory.Generate();
            var cate2 = fakerCategory.Generate();
            var cate21 = fakerCategory.Generate();
            var cate211 = fakerCategory.Generate();

            cate11.ParentCategory = cate;
            cate12.ParentCategory = cate;
            cate21.ParentCategory = cate2;
            cate211.ParentCategory = cate21;

            var categories = new List<Category>() { cate, cate11, cate12, cate2, cate21, cate211 };

            _context.Categories.AddRange(categories);


            var r = new Random();
            var fakerPost = new Faker<Post>();
            int bv = 1;
            fakerPost.RuleFor(c => c.Title, fk => $"Post {bv++} " + fk.Lorem.Sentence(1, 2).Trim('.'));
            fakerPost.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());
            fakerPost.RuleFor(c => c.Description, fk => fk.Lorem.Sentence(5) + "[fakeData]");
            fakerPost.RuleFor(c => c.Content, fk => fk.Lorem.Paragraphs(7));
            fakerPost.RuleFor(c => c.CreatAt, fk => fk.Date.Between(DateTime.SpecifyKind(DateTime.Now.AddYears(-10), DateTimeKind.Utc), DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc)));
            fakerPost.RuleFor(c => c.Published, fk => true);
            fakerPost.RuleFor(c => c.AuthorId, fk => user.Id);

            List<Post> posts = new();
            List<PostCategory> postCategories = new();

            for (int i = 0; i < 40; i++)
            {
                  var post = fakerPost.Generate();
                  post.EditAt = post.CreatAt;
                  posts.Add(post);
                  postCategories.Add(new PostCategory()
                  {
                        Category = categories[r.Next(5)],
                        Post = post
                  });

            }

            _context.Posts.AddRange(posts);
            _context.PostCategories.AddRange(postCategories);

            await _context.SaveChangesAsync();
      }
}