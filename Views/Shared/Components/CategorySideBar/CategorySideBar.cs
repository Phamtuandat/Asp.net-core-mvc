using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;
namespace App.Components
{
    [ViewComponent]
    public class CategorySideBar : ViewComponent
    {
        public class CategorySideBarData
        {
            public List<Category> categories { get; set; }
            public int Level { get; set; }
            public string categorySlug { get; set; }
        }

        public IViewComponentResult Invoke(CategorySideBarData data)
        {
            return View(data);
        }

    }
}