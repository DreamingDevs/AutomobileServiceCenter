using ASC.Models.Models;
using AutoMapper;

namespace ASC.Web.Areas.Promotions.Models
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Promotion, PromotionViewModel>();
            CreateMap<PromotionViewModel, Promotion>();
        }
    }
}
