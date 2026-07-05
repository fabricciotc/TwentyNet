using AutoMapper;
using MediatR;
using TwentyNet.Domain.Interfaces;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Application.Files.ListPersonFiles;

public sealed class ListPersonFilesQueryHandler : IRequestHandler<ListPersonFilesQuery, IReadOnlyList<FileResponse>>
{
    private readonly IRepository<FileEntity> _fileRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListPersonFilesQueryHandler(IRepository<FileEntity> fileRepository, IMapper mapper, IAuthContext authContext)
    {
        _fileRepository = fileRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<FileResponse>> Handle(ListPersonFilesQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var files = await _fileRepository.ListAsync(
            f => f.PersonId == request.PersonId
                 && f.WorkspaceId == _authContext.WorkspaceId.Value
                 && f.DeletedAt == null,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<FileResponse>>(files);
    }
}
