using AutoMapper;
using Domain.Entities;
using Infrastructure.Entities;

namespace Infrastructure.Mapping;

public class TickProfile : Profile
{
    public TickProfile()
    {
        CreateMap<Tick, TickEntity>()
            .ForMember(d => d.Price, opt => opt.MapFrom(m => m.Price))
            .ForMember(d => d.Source, opt => opt.MapFrom(m => m.Source))
            .ForMember(d => d.Symbol, opt => opt.MapFrom(m => m.Symbol))
            .ForMember(d => d.Volume, opt => opt.MapFrom(m => m.Volume))
            .ForMember(d => d.Timestamp, opt => opt.MapFrom(m => m.Timestamp))
            ;
    }
}