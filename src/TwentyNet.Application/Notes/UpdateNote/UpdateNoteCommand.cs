using MediatR;

namespace TwentyNet.Application.Notes.UpdateNote;

public sealed record UpdateNoteCommand(
    Guid Id,
    string Title,
    string Content) : IRequest<NoteDto>;
