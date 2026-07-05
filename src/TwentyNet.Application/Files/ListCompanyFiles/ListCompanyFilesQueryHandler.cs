using AutoMapper;
using MediatR;
using TwentyNet.Domain.Interfaces;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Application.Files.ListCompanyFiles;

public sealed class ListCompanyFilesQueryHandler : IRequestHandler<ListCompanyFilesQuery, IReadOnlyList<FileResponse>>
{
    private readonly IRepository<FileEntity> _fileRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListCompanyFilesQueryHandler(IRepository<FileEntity> fileRepository, IMapper mapper, IAuthContext authContext)
    {
        _fileRepository = fileRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<FileResponse>> Handle(ListCompanyFilesQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var files = await _fileRepository.ListAsync(
            f => f.CompanyId == request.CompanyId
                 && f.WorkspaceId == _authContext.WorkspaceId.Value
                 && f.DeletedAt == null,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<FileResponse>>(files);
    }
}
