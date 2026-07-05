using AutoMapper;
using TwentyNet.Application.Companies;
using TwentyNet.Application.ConnectedAccounts;
using TwentyNet.Application.People;
using TwentyNet.Application.Webhooks;
using TwentyNet.Contracts.Companies;
using TwentyNet.Contracts.ConnectedAccounts;
using TwentyNet.Contracts.People;
using TwentyNet.Contracts.Webhooks;
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
        CreateMap<WebhookDto, WebhookResponse>();
        CreateMap<ConnectedAccountDto, ConnectedAccountResponse>()
            .ForMember(dest => dest.Provider, opt => opt.MapFrom(src => src.Provider.ToString()));
    }
}
