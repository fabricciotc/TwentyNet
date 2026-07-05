using AutoMapper;
using TwentyNet.Application.Companies;
using TwentyNet.Application.ConnectedAccounts;
using TwentyNet.Application.Files;
using TwentyNet.Application.People;
using TwentyNet.Application.Webhooks;
using CompanyEntity = TwentyNet.Domain.Entities.Company;
using ConnectedAccountEntity = TwentyNet.Domain.Entities.ConnectedAccount;
using FileEntity = TwentyNet.Domain.Entities.File;
using PersonEntity = TwentyNet.Domain.Entities.Person;
using WebhookEntity = TwentyNet.Domain.Entities.Webhook;

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
        CreateMap<WebhookEntity, WebhookDto>();
        CreateMap<ConnectedAccountEntity, ConnectedAccountDto>();
    }
}
