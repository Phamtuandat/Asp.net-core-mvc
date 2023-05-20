using App.Areas.Contact.Models;
using App.Models;
using App.Models.Blog;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace App.Data
{

    public class AppDbContext : IdentityDbContext<AppUser>
    {

        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public AppDbContext(DbContextOptions<AppDbContext> options, IWebHostEnvironment env, IConfiguration config) : base(options)
        {
            _env = env;
            _config = config;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            string? connetionString;
            connetionString = _config.GetConnectionString("WebApiDatabase");

            base.OnConfiguring(optionsBuilder);
            optionsBuilder
                    .UseNpgsql(connetionString);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {

            base.OnModelCreating(builder);
            // Bỏ tiền tố AspNet của các bảng: mặc định
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var tableName = entityType.GetTableName();
                if (tableName != null)
                    if (tableName.StartsWith("AspNet"))
                    {
                        entityType.SetTableName(tableName.Substring(6));
                    }
            }

            builder.Entity<Category>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique();
            });
            builder.Entity<PostCategory>(entity =>
            {
                entity.HasKey(c => new { c.CategoryID, c.PostID });
            });

            builder.Entity<Post>(entity =>
            {
                entity.HasIndex(c => c.Slug).IsUnique();
            });
        }


        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<Post> Posts { get; set; }
    };
}
