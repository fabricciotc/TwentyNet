using MediatR;
using TwentyNet.Domain.Interfaces;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Application.Files.CreateFileUpload;

public sealed class CreateFileUploadCommandHandler : IRequestHandler<CreateFileUploadCommand, FileUploadResponse>
{
    private readonly IRepository<FileEntity> _fileRepository;
    private readonly IStorageDriver _storageDriver;
    private readonly IAuthContext _authContext;

    public CreateFileUploadCommandHandler(IRepository<FileEntity> fileRepository, IStorageDriver storageDriver, IAuthContext authContext)
    {
        _fileRepository = fileRepository;
        _storageDriver = storageDriver;
        _authContext = authContext;
    }

    public async Task<FileUploadResponse> Handle(CreateFileUploadCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;
        var file = new FileEntity
        {
            Name = request.Name,
            MimeType = request.MimeType,
            Size = request.Size,
            Folder = request.Folder,
            WorkspaceId = workspaceId,
            StorageKey = string.Empty
        };

        file.StorageKey = $"{workspaceId}/{request.Folder.ToString().ToLowerInvariant()}/{file.Id}-{request.Name}";

        await _fileRepository.AddAsync(file, cancellationToken);
        await _fileRepository.SaveChangesAsync(cancellationToken);

        var uploadUrl = await _storageDriver.GetPresignedUploadUrlAsync(file.StorageKey, TimeSpan.FromMinutes(15), cancellationToken);

        uploadUrl ??= $"/api/files/{file.Id}/content";

        return new FileUploadResponse(file.Id, uploadUrl, file.StorageKey);
    }
}
