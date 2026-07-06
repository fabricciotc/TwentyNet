using System.Globalization;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.Extensions.Logging;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.ImportExport.ImportCompanies;

public sealed class ImportCompaniesCommandHandler : IRequestHandler<ImportCompaniesCommand, ImportResult>
{
    private readonly IRepository<Company> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;
    private readonly IRealTimeNotifier _realTimeNotifier;
    private readonly IPublisher _publisher;
    private readonly ILogger<ImportCompaniesCommandHandler> _logger;

    public ImportCompaniesCommandHandler(
        IRepository<Company> repository,
        IMapper mapper,
        IAuthContext authContext,
        IRealTimeNotifier realTimeNotifier,
        IPublisher publisher,
        ILogger<ImportCompaniesCommandHandler> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
        _realTimeNotifier = realTimeNotifier;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<ImportResult> Handle(ImportCompaniesCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;
        var errors = new List<string>();
        var created = 0;
        var updated = 0;

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim()
        };

        using var reader = new StreamReader(request.CsvStream);
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<CompanyImportRecord>();
        var rowNumber = 1;

        foreach (var record in records)
        {
            rowNumber++;

            if (string.IsNullOrWhiteSpace(record.Name))
            {
                errors.Add($"Row {rowNumber}: Name is required.");
                continue;
            }

            var name = record.Name.Trim();
            var existingList = await _repository.ListAsync(
                c => c.WorkspaceId == workspaceId && c.Name == name,
                cancellationToken);
            var existing = existingList.FirstOrDefault();

            try
            {
                if (existing is not null)
                {
                    existing.Name = name;
                    existing.DomainName = record.DomainName?.Trim();
                    existing.Address = record.Address?.Trim();
                    _repository.Update(existing);
                    updated++;

                    await _repository.SaveChangesAsync(cancellationToken);

                    var updatedEvent = new ObjectRecordUpdatedEvent(workspaceId, "Company", existing.Id);
                    await _realTimeNotifier.NotifyAsync(updatedEvent, cancellationToken);
                    await _publisher.Publish(updatedEvent, cancellationToken);
                }
                else
                {
                    var company = new Company
                    {
                        Name = name,
                        DomainName = record.DomainName?.Trim(),
                        Address = record.Address?.Trim(),
                        WorkspaceId = workspaceId
                    };

                    await _repository.AddAsync(company, cancellationToken);
                    await _repository.SaveChangesAsync(cancellationToken);
                    created++;

                    var createdEvent = new ObjectRecordCreatedEvent(workspaceId, "Company", company.Id);
                    await _realTimeNotifier.NotifyAsync(createdEvent, cancellationToken);
                    await _publisher.Publish(createdEvent, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import company at row {RowNumber}", rowNumber);
                errors.Add($"Row {rowNumber}: {ex.Message}");
            }
        }

        return new ImportResult(created, updated, 0, errors);
    }
}
