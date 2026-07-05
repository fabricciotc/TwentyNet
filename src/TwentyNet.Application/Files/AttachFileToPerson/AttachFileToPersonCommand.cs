using MediatR;

namespace TwentyNet.Application.Files.AttachFileToPerson;

public sealed record AttachFileToPersonCommand(Guid PersonId, Guid FileId) : IRequest;
