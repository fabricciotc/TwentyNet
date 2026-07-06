using System.Text.Json;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.UpdateCompanyCustomFields;

public sealed class UpdateCompanyCustomFieldsCommandHandler : IRequestHandler<UpdateCompanyCustomFieldsCommand>
{
    private readonly IRepository<Company> _repository;
    private readonly IAuthContext _authContext;

    public UpdateCompanyCustomFieldsCommandHandler(
        IRepository<Company> repository,
        IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task Handle(UpdateCompanyCustomFieldsCommand request, CancellationToken cancellationToken)
    {
        var workspaceId = _authContext.WorkspaceId
            ?? throw new UnauthorizedAccessException("Workspace not selected.");

        var companies = await _repository.ListAsync(
            x => x.Id == request.CompanyId && x.WorkspaceId == workspaceId,
            cancellationToken);

        var company = companies.FirstOrDefault()
            ?? throw new InvalidOperationException("Company not found.");

        company.CustomFields = JsonSerializer.Serialize(request.CustomFields);

        await _repository.SaveChangesAsync(cancellationToken);
    }
}
