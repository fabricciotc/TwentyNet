using MediatR;

namespace TwentyNet.Application.Notes.GetNoteById;

public sealed record GetNoteByIdQuery(Guid Id) : IRequest<NoteDto?>;
