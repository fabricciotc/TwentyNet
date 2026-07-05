using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Notes.DeleteNote;

public sealed class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand>
{
    private readonly IRepository<Note> _noteRepository;
    private readonly IAuthContext _authContext;

    public DeleteNoteCommandHandler(IRepository<Note> noteRepository, IAuthContext authContext)
    {
        _noteRepository = noteRepository;
        _authContext = authContext;
    }

    public async Task Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
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

        await _noteRepository.DeleteAsync(note.Id, cancellationToken);
        await _noteRepository.SaveChangesAsync(cancellationToken);
    }
}
