using App.Data;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("MVCIdentityDbContextConnection") ?? throw new InvalidOperationException("Connection string 'MVCIdentityDbContextConnection' not found.");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddIdentity<AppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
builder.Services.AddScoped<IEmailSender, SendMailService>();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);

    options.AccessDeniedPath = "/AccessDenied";
    options.SlidingExpiration = true;
});
builder.Services
        .AddAuthentication()
        .AddGoogle(option =>
        {
            var gconfig = builder.Configuration.GetSection("Authentication:Google");
            option.ClientId = gconfig["ClientId"];
            option.ClientSecret = gconfig["ClientSecrect"];
            option.CallbackPath = "/login-with-google";
        });
builder.Services.AddAuthorization(option =>
{
    option.AddPolicy("ViewManageMenu", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireRole(RoleNames.Administrator);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.AddStatusCodePage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();   // Phục hồi thông tin đăng nhập (xác thực)
app.UseAuthorization();
app.MapAreaControllerRoute(
    name: "contact",
    pattern: "/{controller}/{action=Index}/{id?}",
    areaName: "Contact"
    );
app.MapAreaControllerRoute(
    name: "database",
    pattern: "/{controller}/{action=Index}/{id?}",
    areaName: "Database"
);


app.MapControllerRoute(
    name: "default",
    pattern: "/{controller=Home}/{action=Index}/{id?}");
app.Run();
