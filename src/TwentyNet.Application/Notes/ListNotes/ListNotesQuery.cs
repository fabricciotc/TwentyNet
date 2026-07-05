using MediatR;

namespace TwentyNet.Application.Notes.ListNotes;

public sealed record ListNotesQuery(
    Guid? CompanyId,
    Guid? PersonId) : IRequest<IReadOnlyList<NoteDto>>;
