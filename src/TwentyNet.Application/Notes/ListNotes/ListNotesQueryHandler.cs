using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Notes.ListNotes;

public sealed class ListNotesQueryHandler : IRequestHandler<ListNotesQuery, IReadOnlyList<NoteDto>>
{
    private readonly IRepository<Note> _noteRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListNotesQueryHandler(IRepository<Note> noteRepository, IMapper mapper, IAuthContext authContext)
    {
        _noteRepository = noteRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<NoteDto>> Handle(ListNotesQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;

        var notes = await _noteRepository.ListAsync(
            n => n.WorkspaceId == workspaceId
                 && (!request.CompanyId.HasValue || n.CompanyId == request.CompanyId.Value)
                 && (!request.PersonId.HasValue || n.PersonId == request.PersonId.Value),
            cancellationToken);

        return _mapper.Map<IReadOnlyList<NoteDto>>(notes);
    }
}
