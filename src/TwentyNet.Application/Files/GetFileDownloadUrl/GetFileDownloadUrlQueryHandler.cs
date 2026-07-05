using MediatR;
using TwentyNet.Domain.Interfaces;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Application.Files.GetFileDownloadUrl;

public sealed class GetFileDownloadUrlQueryHandler : IRequestHandler<GetFileDownloadUrlQuery, FileDownloadUrlResponse>
{
    private readonly IRepository<FileEntity> _fileRepository;
    private readonly IStorageDriver _storageDriver;
    private readonly IAuthContext _authContext;

    public GetFileDownloadUrlQueryHandler(IRepository<FileEntity> fileRepository, IStorageDriver storageDriver, IAuthContext authContext)
    {
        _fileRepository = fileRepository;
        _storageDriver = storageDriver;
        _authContext = authContext;
    }

    public async Task<FileDownloadUrlResponse> Handle(GetFileDownloadUrlQuery request, CancellationToken cancellationToken)
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

        var url = await _storageDriver.GetPresignedDownloadUrlAsync(file.StorageKey, TimeSpan.FromMinutes(15), cancellationToken);

        return new FileDownloadUrlResponse(url, file.StorageKey);
    }
}
