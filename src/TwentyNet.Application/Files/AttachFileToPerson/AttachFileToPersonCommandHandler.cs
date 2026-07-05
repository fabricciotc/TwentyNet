using MediatR;
using TwentyNet.Domain.Interfaces;
using FileEntity = TwentyNet.Domain.Entities.File;
using PersonEntity = TwentyNet.Domain.Entities.Person;

namespace TwentyNet.Application.Files.AttachFileToPerson;

public sealed class AttachFileToPersonCommandHandler : IRequestHandler<AttachFileToPersonCommand>
{
    private readonly IRepository<FileEntity> _fileRepository;
    private readonly IRepository<PersonEntity> _personRepository;
    private readonly IAuthContext _authContext;

    public AttachFileToPersonCommandHandler(IRepository<FileEntity> fileRepository, IRepository<Domain.Entities.Person> personRepository, IAuthContext authContext)
    {
        _fileRepository = fileRepository;
        _personRepository = personRepository;
        _authContext = authContext;
    }

    public async Task Handle(AttachFileToPersonCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;

        var people = await _personRepository.ListAsync(
            p => p.Id == request.PersonId && p.WorkspaceId == workspaceId,
            cancellationToken);

        var person = people.FirstOrDefault();

        if (person is null)
        {
            throw new KeyNotFoundException($"Person with id {request.PersonId} not found.");
        }

        var files = await _fileRepository.ListAsync(
            f => f.Id == request.FileId
                 && f.WorkspaceId == workspaceId
                 && f.DeletedAt == null,
            cancellationToken);

        var file = files.FirstOrDefault();

        if (file is null)
        {
            throw new KeyNotFoundException($"File with id {request.FileId} not found.");
        }

        file.PersonId = person.Id;
        file.CompanyId = null;
        _fileRepository.Update(file);
        await _fileRepository.SaveChangesAsync(cancellationToken);
    }
}
