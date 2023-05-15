using Microsoft.AspNetCore.Identity;
namespace App.Models
{
    public class AppUser : IdentityUser
    {
        public DateOnly? BirthDate { get; internal set; }
        public string? HomeAdress { get; internal set; } = string.Empty;
    }
}