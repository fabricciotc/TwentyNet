using AutoMapper;
using TwentyNet.Application.Companies;
using TwentyNet.Application.ConnectedAccounts;
using TwentyNet.Application.Files;
using TwentyNet.Application.Notes;
using TwentyNet.Application.People;
using TwentyNet.Application.Tasks;
using TwentyNet.Application.Timeline;
using TwentyNet.Application.Views;
using TwentyNet.Application.Webhooks;
using TwentyNet.Contracts.Companies;
using TwentyNet.Contracts.ConnectedAccounts;
using TwentyNet.Contracts.People;
using TwentyNet.Contracts.Views;
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
        CreateMap<ViewDto, ViewResponse>();
        CreateMap<ViewFilterDto, ViewFilterResponse>();
        CreateMap<ViewSortDto, ViewSortResponse>();
        CreateMap<NoteDto, Contracts.Notes.NoteResponse>();
        CreateMap<TaskDto, Contracts.Tasks.TaskResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));
        CreateMap<TimelineActivityDto, Contracts.Timeline.TimelineActivityResponse>()
            .ForMember(dest => dest.ActivityType, opt => opt.MapFrom(src => src.ActivityType));
        CreateMap(typeof(Application.Common.PagedResult<>), typeof(Contracts.Common.PagedResponse<>));
    }
}
