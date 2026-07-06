using System.Globalization;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.Extensions.Logging;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;

namespace TwentyNet.Application.ImportExport.ImportPeople;

public sealed class ImportPeopleCommandHandler : IRequestHandler<ImportPeopleCommand, ImportResult>
{
    private readonly IRepository<Person> _personRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;
    private readonly IRealTimeNotifier _realTimeNotifier;
    private readonly IPublisher _publisher;
    private readonly ILogger<ImportPeopleCommandHandler> _logger;

    public ImportPeopleCommandHandler(
        IRepository<Person> personRepository,
        IRepository<Company> companyRepository,
        IMapper mapper,
        IAuthContext authContext,
        IRealTimeNotifier realTimeNotifier,
        IPublisher publisher,
        ILogger<ImportPeopleCommandHandler> logger)
    {
        _personRepository = personRepository;
        _companyRepository = companyRepository;
        _mapper = mapper;
        _authContext = authContext;
        _realTimeNotifier = realTimeNotifier;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<ImportResult> Handle(ImportPeopleCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;
        var errors = new List<string>();
        var created = 0;
        var updated = 0;

        var companies = await _companyRepository.ListAsync(
            c => c.WorkspaceId == workspaceId,
            cancellationToken);
        var companyByName = companies.ToDictionary(c => c.Name.Trim(), StringComparer.OrdinalIgnoreCase);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            PrepareHeaderForMatch = args => args.Header.Trim()
        };

        using var reader = new StreamReader(request.CsvStream);
        using var csv = new CsvReader(reader, config);
        var records = csv.GetRecords<PersonImportRecord>();
        var rowNumber = 1;

        foreach (var record in records)
        {
            rowNumber++;

            if (string.IsNullOrWhiteSpace(record.FirstName) || string.IsNullOrWhiteSpace(record.LastName))
            {
                errors.Add($"Row {rowNumber}: FirstName and LastName are required.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(record.Email))
            {
                errors.Add($"Row {rowNumber}: Email is required.");
                continue;
            }

            Email? email;
            try
            {
                email = new Email(record.Email.Trim());
            }
            catch (ArgumentException ex)
            {
                errors.Add($"Row {rowNumber}: {ex.Message}");
                continue;
            }

            PhoneNumber? phone = null;
            if (!string.IsNullOrWhiteSpace(record.Phone))
            {
                try
                {
                    phone = new PhoneNumber(record.Phone.Trim());
                }
                catch (ArgumentException ex)
                {
                    errors.Add($"Row {rowNumber}: {ex.Message}");
                    continue;
                }
            }

            Guid? companyId = null;
            if (!string.IsNullOrWhiteSpace(record.CompanyName)
                && companyByName.TryGetValue(record.CompanyName.Trim(), out var matchedCompany))
            {
                companyId = matchedCompany.Id;
            }

            var emailValue = email.Value;
            var existingList = await _personRepository.ListAsync(
                p => p.WorkspaceId == workspaceId && p.Email != null && p.Email.Value == emailValue,
                cancellationToken);
            var existing = existingList.FirstOrDefault();

            try
            {
                if (existing is not null)
                {
                    existing.FirstName = record.FirstName.Trim();
                    existing.LastName = record.LastName.Trim();
                    existing.Email = email;
                    existing.Phone = phone;
                    existing.CompanyId = companyId;
                    _personRepository.Update(existing);
                    updated++;

                    await _personRepository.SaveChangesAsync(cancellationToken);

                    var updatedEvent = new ObjectRecordUpdatedEvent(workspaceId, "Person", existing.Id);
                    await _realTimeNotifier.NotifyAsync(updatedEvent, cancellationToken);
                    await _publisher.Publish(updatedEvent, cancellationToken);
                }
                else
                {
                    var person = new Person
                    {
                        FirstName = record.FirstName.Trim(),
                        LastName = record.LastName.Trim(),
                        Email = email,
                        Phone = phone,
                        CompanyId = companyId,
                        WorkspaceId = workspaceId
                    };

                    await _personRepository.AddAsync(person, cancellationToken);
                    await _personRepository.SaveChangesAsync(cancellationToken);
                    created++;

                    var createdEvent = new ObjectRecordCreatedEvent(workspaceId, "Person", person.Id);
                    await _realTimeNotifier.NotifyAsync(createdEvent, cancellationToken);
                    await _publisher.Publish(createdEvent, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to import person at row {RowNumber}", rowNumber);
                errors.Add($"Row {rowNumber}: {ex.Message}");
            }
        }

        return new ImportResult(created, updated, 0, errors);
    }
}
