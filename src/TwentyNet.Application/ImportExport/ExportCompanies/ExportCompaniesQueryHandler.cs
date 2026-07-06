using System.Globalization;
using CsvHelper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.ImportExport.ExportCompanies;

public sealed class ExportCompaniesQueryHandler : IRequestHandler<ExportCompaniesQuery, byte[]>
{
    private readonly IRepository<Company> _repository;
    private readonly IAuthContext _authContext;

    public ExportCompaniesQueryHandler(IRepository<Company> repository, IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task<byte[]> Handle(ExportCompaniesQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;
        var companies = await _repository.ListAsync(
            c => c.WorkspaceId == workspaceId,
            cancellationToken);

        var records = companies.Select(c => new CompanyImportRecord
        {
            Name = c.Name,
            DomainName = c.DomainName,
            Address = c.Address
        });

        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(records, cancellationToken);
        await csv.FlushAsync();

        return writer.Encoding.GetBytes(writer.ToString());
    }
}
