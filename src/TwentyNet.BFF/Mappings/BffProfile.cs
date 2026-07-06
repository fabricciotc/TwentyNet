using AutoMapper;
using TwentyNet.Application.Companies;
using TwentyNet.Application.ConnectedAccounts;
using TwentyNet.Application.Files;
using TwentyNet.Application.Notes;
using TwentyNet.Application.People;
using TwentyNet.Application.Tasks;
using TwentyNet.Application.Timeline;
using TwentyNet.Application.Views;
using TwentyNet.Application.ApiKeys;
using TwentyNet.Application.Chatbot;
using TwentyNet.Application.Sso;
using TwentyNet.Application.Sync;
using TwentyNet.Application.Webhooks;
using TwentyNet.Application.Workflows;
using TwentyNet.Contracts.Companies;
using TwentyNet.Contracts.ConnectedAccounts;
using TwentyNet.Contracts.People;
using TwentyNet.Contracts.Views;
using TwentyNet.Contracts.ApiKeys;
using TwentyNet.Contracts.Chatbot;
using TwentyNet.Contracts.Sso;
using TwentyNet.Contracts.Sync;
using TwentyNet.Contracts.Webhooks;
using TwentyNet.Contracts.Workflows;
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

        CreateMap<WorkflowDto, WorkflowResponse>()
            .ForMember(dest => dest.TriggerType, opt => opt.MapFrom(src => src.TriggerType.ToString()));
        CreateMap<TwentyNet.Domain.Workflows.WorkflowActionConfig, WorkflowActionResponse>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
        CreateMap<WorkflowActionRequest, TwentyNet.Domain.Workflows.WorkflowActionConfig>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => Enum.Parse<TwentyNet.Domain.Enums.WorkflowActionType>(src.Type, true)));

        CreateMap<ApiKeyDto, ApiKeyResponse>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role.ToString()));
        CreateMap<ApiKeyCreatedDto, ApiKeyCreatedResponse>();

        CreateMap<SsoProviderDto, SsoProviderResponse>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<EmailMessageDto, EmailMessageResponse>();
        CreateMap<CalendarEventDto, CalendarEventResponse>();

        CreateMap<ChatSessionDto, ChatSessionResponse>();
        CreateMap<ChatMessageDto, ChatMessageResponse>();
    }
}
