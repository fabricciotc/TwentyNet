using MediatR;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Application.Files.CompleteFileUpload;

public sealed class CompleteFileUploadCommandHandler : IRequestHandler<CompleteFileUploadCommand>
{
    private readonly IRepository<FileEntity> _fileRepository;
    private readonly IAuthContext _authContext;

    public CompleteFileUploadCommandHandler(IRepository<Domain.Entities.File> fileRepository, IAuthContext authContext)
    {
        _fileRepository = fileRepository;
        _authContext = authContext;
    }

    public async Task Handle(CompleteFileUploadCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var files = await _fileRepository.ListAsync(
            f => f.Id == request.FileId
                 && f.WorkspaceId == _authContext.WorkspaceId.Value
                 && f.DeletedAt == null,
            cancellationToken);

        var file = files.FirstOrDefault();

        if (file is null)
        {
            throw new KeyNotFoundException($"File with id {request.FileId} not found.");
        }

        if (file.Status != FileStatus.Pending)
        {
            throw new InvalidOperationException($"File with id {request.FileId} is not pending.");
        }

        file.Status = FileStatus.Uploaded;
        _fileRepository.Update(file);
        await _fileRepository.SaveChangesAsync(cancellationToken);
    }
}
