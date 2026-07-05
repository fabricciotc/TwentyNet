using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Notes.UpdateNote;

public sealed class UpdateNoteCommandHandler : IRequestHandler<UpdateNoteCommand, NoteDto>
{
    private readonly IRepository<Note> _noteRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public UpdateNoteCommandHandler(
        IRepository<Note> noteRepository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _noteRepository = noteRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<NoteDto> Handle(UpdateNoteCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var notes = await _noteRepository.ListAsync(
            n => n.Id == request.Id && n.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var note = notes.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Note with id {request.Id} not found.");

        note.Title = request.Title;
        note.Content = request.Content;

        _noteRepository.Update(note);
        await _noteRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<NoteDto>(note);
    }
}
