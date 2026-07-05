using AutoMapper;
using TwentyNet.Application.Companies;
using TwentyNet.Application.People;
using TwentyNet.Domain.Entities;

namespace TwentyNet.Application.Mappings;

public sealed class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        CreateMap<Company, CompanyDto>();
        CreateMap<Person, PersonDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email != null ? src.Email.Value : null))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Value : null));
    }
}
