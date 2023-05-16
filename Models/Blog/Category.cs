using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.Blog
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.Text)]
        public string Description { set; get; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [RegularExpression(@"^[a-z0-9-]*$")]
        [Display(Name = "Url")]
        public string Slug { set; get; } = string.Empty;


        [Display(Name = "Parent Category")]
        public int? ParentCategoryId { get; set; }
        public ICollection<Category>? CategoryChildren { get; set; }

        [ForeignKey("ParentCategoryId")]
        [Display(Name = "Parent Category")]
        public Category? ParentCategory { set; get; }
    }
}