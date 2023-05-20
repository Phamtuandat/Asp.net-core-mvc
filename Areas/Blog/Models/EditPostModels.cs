using App.Models;

namespace App.Areas.Blog.Models
{
    public class EditPostModels : CreatePostModels
    {
        public int PostId { get; set; }
    }
}