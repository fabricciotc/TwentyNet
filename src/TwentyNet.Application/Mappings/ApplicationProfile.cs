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
using TwentyNet.Application.Workspaces;
using CompanyEntity = TwentyNet.Domain.Entities.Company;
using ConnectedAccountEntity = TwentyNet.Domain.Entities.ConnectedAccount;
using FileEntity = TwentyNet.Domain.Entities.File;
using NoteEntity = TwentyNet.Domain.Entities.Note;
using PersonEntity = TwentyNet.Domain.Entities.Person;
using TaskItemEntity = TwentyNet.Domain.Entities.TaskItem;
using TimelineActivityEntity = TwentyNet.Domain.Entities.TimelineActivity;
using ViewEntity = TwentyNet.Domain.Entities.View;
using ViewFilterEntity = TwentyNet.Domain.Entities.ViewFilter;
using ViewSortEntity = TwentyNet.Domain.Entities.ViewSort;
using WebhookEntity = TwentyNet.Domain.Entities.Webhook;
using WorkspaceInviteEntity = TwentyNet.Domain.Entities.WorkspaceInvite;

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
        CreateMap<WorkspaceInviteEntity, WorkspaceInviteDto>();
        CreateMap<ViewEntity, ViewDto>();
        CreateMap<ViewFilterEntity, ViewFilterDto>();
        CreateMap<ViewSortEntity, ViewSortDto>();
        CreateMap<NoteEntity, NoteDto>();
        CreateMap<TaskItemEntity, TaskDto>();
        CreateMap<TimelineActivityEntity, TimelineActivityDto>();
    }
}
