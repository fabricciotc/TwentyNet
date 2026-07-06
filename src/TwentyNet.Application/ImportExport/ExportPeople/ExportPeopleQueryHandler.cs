using System.Globalization;
using CsvHelper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.ImportExport.ExportPeople;

public sealed class ExportPeopleQueryHandler : IRequestHandler<ExportPeopleQuery, byte[]>
{
    private readonly IRepository<Person> _personRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IAuthContext _authContext;

    public ExportPeopleQueryHandler(
        IRepository<Person> personRepository,
        IRepository<Company> companyRepository,
        IAuthContext authContext)
    {
        _personRepository = personRepository;
        _companyRepository = companyRepository;
        _authContext = authContext;
    }

    public async Task<byte[]> Handle(ExportPeopleQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;
        var people = await _personRepository.ListAsync(
            p => p.WorkspaceId == workspaceId,
            cancellationToken);

        var companyIds = people
            .Where(p => p.CompanyId.HasValue)
            .Select(p => p.CompanyId!.Value)
            .Distinct()
            .ToList();

        var companies = await _companyRepository.ListAsync(
            c => companyIds.Contains(c.Id) && c.WorkspaceId == workspaceId,
            cancellationToken);
        var companyById = companies.ToDictionary(c => c.Id);

        var records = people.Select(p => new PersonImportRecord
        {
            FirstName = p.FirstName,
            LastName = p.LastName,
            Email = p.Email?.Value ?? string.Empty,
            Phone = p.Phone?.Value,
            CompanyName = p.CompanyId.HasValue && companyById.TryGetValue(p.CompanyId.Value, out var company)
                ? company.Name
                : null
        });

        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(records, cancellationToken);
        await csv.FlushAsync();

        return writer.Encoding.GetBytes(writer.ToString());
    }
}
