using App.Data;
using App.Models;
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
        await SeedData();
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
            userAdmin = new AppUser()
            {
                UserName = "admin",
                Email = "admin@admin.com",
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(userAdmin, "Phamdat11a1.");
            _logger.LogInformation(userAdmin.Email);
            await _userManager.AddToRoleAsync(userAdmin, RoleNames.Administrator);
        }
        return RedirectToAction(nameof(Index));
    }
}