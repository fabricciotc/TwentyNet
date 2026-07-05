using AutoMapper;
using TwentyNet.Application.Companies;
using TwentyNet.Application.Files;
using TwentyNet.Application.People;
using CompanyEntity = TwentyNet.Domain.Entities.Company;
using FileEntity = TwentyNet.Domain.Entities.File;
using PersonEntity = TwentyNet.Domain.Entities.Person;

namespace TwentyNet.Application.Mappings;

public sealed class ApplicationProfile : Profile
{
    public ApplicationProfile()
    {
        CreateMap<CompanyEntity, CompanyDto>();
        CreateMap<PersonEntity, PersonDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email != null ? src.Email.Value : null))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Value : null));
        CreateMap<FileEntity, FileResponse>();
    }
}
