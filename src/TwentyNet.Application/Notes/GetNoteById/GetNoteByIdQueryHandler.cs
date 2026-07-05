using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Notes.GetNoteById;

public sealed class GetNoteByIdQueryHandler : IRequestHandler<GetNoteByIdQuery, NoteDto?>
{
    private readonly IRepository<Note> _noteRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetNoteByIdQueryHandler(IRepository<Note> noteRepository, IMapper mapper, IAuthContext authContext)
    {
        _noteRepository = noteRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<NoteDto?> Handle(GetNoteByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var notes = await _noteRepository.ListAsync(
            n => n.Id == request.Id && n.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var note = notes.FirstOrDefault();
        return note is null ? null : _mapper.Map<NoteDto>(note);
    }
}
