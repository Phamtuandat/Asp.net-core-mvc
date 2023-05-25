using App.Areas.Blog.Models;
using App.Models;
using AutoMapper;

namespace App.Extensions.AutoMapper
{

    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreatePostModels, Post>();
            CreateMap<EditPostModels, Post>();
            CreateMap<Post, EditPostModels>();


        }
    }
}