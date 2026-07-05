using MediatR;
using TwentyNet.Domain.Interfaces;
using FileEntity = TwentyNet.Domain.Entities.File;
using CompanyEntity = TwentyNet.Domain.Entities.Company;

namespace TwentyNet.Application.Files.AttachFileToCompany;

public sealed class AttachFileToCompanyCommandHandler : IRequestHandler<AttachFileToCompanyCommand>
{
    private readonly IRepository<FileEntity> _fileRepository;
    private readonly IRepository<CompanyEntity> _companyRepository;
    private readonly IAuthContext _authContext;

    public AttachFileToCompanyCommandHandler(IRepository<FileEntity> fileRepository, IRepository<Domain.Entities.Company> companyRepository, IAuthContext authContext)
    {
        _fileRepository = fileRepository;
        _companyRepository = companyRepository;
        _authContext = authContext;
    }

    public async Task Handle(AttachFileToCompanyCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;

        var companies = await _companyRepository.ListAsync(
            c => c.Id == request.CompanyId && c.WorkspaceId == workspaceId,
            cancellationToken);

        var company = companies.FirstOrDefault();

        if (company is null)
        {
            throw new KeyNotFoundException($"Company with id {request.CompanyId} not found.");
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

        file.CompanyId = company.Id;
        file.PersonId = null;
        _fileRepository.Update(file);
        await _fileRepository.SaveChangesAsync(cancellationToken);
    }
}
