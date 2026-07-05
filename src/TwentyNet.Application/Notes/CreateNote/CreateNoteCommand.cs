using MediatR;

namespace TwentyNet.Application.Notes.CreateNote;

public sealed record CreateNoteCommand(
    string Title,
    string Content,
    Guid? CompanyId,
    Guid? PersonId) : IRequest<NoteDto>;
