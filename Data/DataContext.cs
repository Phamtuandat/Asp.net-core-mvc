using App.Areas.Contact.Models;
using App.Models;
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
        }


        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
    };
}
