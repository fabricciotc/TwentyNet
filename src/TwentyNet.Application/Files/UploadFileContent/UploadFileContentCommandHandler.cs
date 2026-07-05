using MediatR;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Application.Files.UploadFileContent;

public sealed class UploadFileContentCommandHandler : IRequestHandler<UploadFileContentCommand>
{
    private readonly IRepository<FileEntity> _fileRepository;
    private readonly IStorageDriver _storageDriver;
    private readonly IAuthContext _authContext;

    public UploadFileContentCommandHandler(IRepository<FileEntity> fileRepository, IStorageDriver storageDriver, IAuthContext authContext)
    {
        _fileRepository = fileRepository;
        _storageDriver = storageDriver;
        _authContext = authContext;
    }

    public async Task Handle(UploadFileContentCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var files = await _fileRepository.ListAsync(
            f => f.Id == request.FileId
                 && f.WorkspaceId == _authContext.WorkspaceId.Value
                 && f.Status == FileStatus.Pending
                 && f.DeletedAt == null,
            cancellationToken);

        var file = files.FirstOrDefault();

        if (file is null)
        {
            throw new KeyNotFoundException($"Pending file with id {request.FileId} not found.");
        }

        await _storageDriver.WriteAsync(file.StorageKey, request.Content, request.ContentType, cancellationToken);
    }
}
