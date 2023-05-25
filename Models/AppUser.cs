using Microsoft.AspNetCore.Identity;
namespace App.Models
{
      public class AppUser : IdentityUser
      {
            public string? FirstName { get; set; }
            public string? LastName { get; set; }
            public DateOnly? BirthDate { get; set; }
            public string? HomeAdress { get; set; } = string.Empty;
      }
}