using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models
{
    public class Post
    {
        [Key]
        public int PostId { set; get; }

        [Required]
        [StringLength(160, MinimumLength = 5)]
        public string Title { set; get; } = string.Empty;

        public string Description { set; get; } = string.Empty;

        public bool Published { set; get; }


        [StringLength(160, MinimumLength = 5)]
        [RegularExpression(@"^[a-z0-9-]*$")]
        public string Slug { set; get; }

        public string Content { set; get; } = string.Empty;

        public string? AuthorId { set; get; }
        [ForeignKey("AuthorId")]
        [Display(Name = "Author")]
        public AppUser? Author { set; get; }

        public IEnumerable<PostCategory> PostCategories { get; set; } = new List<PostCategory>();

        public DateTime CreatAt { get; set; }
        public DateTime? EditAt { get; set; }
    }
}