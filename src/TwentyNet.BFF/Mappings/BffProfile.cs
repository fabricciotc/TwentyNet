using AutoMapper;
using TwentyNet.Application.Companies;
using TwentyNet.Application.People;
using TwentyNet.Contracts.Companies;
using TwentyNet.Contracts.People;
using ApplicationFileResponse = TwentyNet.Application.Files.FileResponse;
using ApplicationFileUploadResponse = TwentyNet.Application.Files.FileUploadResponse;

namespace TwentyNet.BFF.Mappings;

public sealed class BffProfile : Profile
{
    public BffProfile()
    {
        CreateMap<CompanyDto, CompanyResponse>();
        CreateMap<PersonDto, PersonResponse>();
        CreateMap<ApplicationFileResponse, Contracts.Files.FileResponse>()
            .ForMember(dest => dest.Folder, opt => opt.MapFrom(src => src.Folder.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        CreateMap<ApplicationFileUploadResponse, Contracts.Files.FileUploadResponse>();
    }
}
