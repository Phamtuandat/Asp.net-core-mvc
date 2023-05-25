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

    public void CategoryChildrenIds(ICollection<Category>? categories, List<int> ids)
    {
      if (categories == null)
        categories = this.CategoryChildren;
      foreach (var item in categories)
      {
        ids.Add(item.Id);
        if (item.CategoryChildren?.Count > 0)
        {
          CategoryChildrenIds(item.CategoryChildren, ids);
        }

      }
    }

    public List<Category> ParentCategories()
    {
      List<Category> li = new List<Category>();
      var parent = this.ParentCategory;
      while (parent != null)
      {
        li.Add(parent);
        parent = parent.ParentCategory;
      }
      li.Reverse();
      return li;
    }
  }


  public class CategorySelecItem : Category
  {
    public int level { get; set; }
  }
}