using AutoMapper;
using MVC.ViewModels.Inbound;
using Hassann_Khala.Domain;

namespace MVC.Mapping
{
    public class InboundProfile : Profile
    {
        public InboundProfile()
        {
            CreateMap<Inbound, InboundListVM>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.Name))
                .ForMember(dest => dest.LinesCount, opt => opt.MapFrom(src => src.Details.Count));

            CreateMap<Inbound, InboundDetailsVM>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client.Name))
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.Details));

            CreateMap<InboundDetail, InboundDetailVM>()
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
                .ForMember(dest => dest.SectionId, opt => opt.MapFrom(src => src.SectionId))
                .ForMember(dest => dest.Cartons, opt => opt.MapFrom(src => src.Cartons))
                .ForMember(dest => dest.Pallets, opt => opt.MapFrom(src => src.Pallets))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
                .ForMember(dest => dest.SectionName, opt => opt.MapFrom(src => src.Section != null ? src.Section.Name : null));
        }
    }
}