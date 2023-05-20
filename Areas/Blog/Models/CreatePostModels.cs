using System.ComponentModel.DataAnnotations;
using App.Models;

namespace App.Areas.Blog.Models

{
    public class CreatePostModels
    {

        [Required]
        [StringLength(160, MinimumLength = 5)]
        public string Title { set; get; } = string.Empty;

        public string Description { set; get; } = string.Empty;
        public int[]? CategoryIDs { get; set; }
        public bool Published { set; get; }


        [StringLength(160, MinimumLength = 5)]
        [RegularExpression(@"^[a-z0-9-]*$")]
        public string? Slug { set; get; }

        public string Content { set; get; } = string.Empty;
    }
}