using Microsoft.AspNetCore.Identity;
namespace App.Models
{
    public class AppUser : IdentityUser
    {
        public DateOnly? BirthDate { get; set; }
        public string? HomeAdress { get; set; } = string.Empty;
    }
}