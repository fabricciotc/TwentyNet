using MediatR;

namespace TwentyNet.Application.Notes.DeleteNote;

public sealed record DeleteNoteCommand(Guid Id) : IRequest;
