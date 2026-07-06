using MediatR;

namespace TwentyNet.Application.ImportExport.ImportCompanies;

public sealed record ImportCompaniesCommand(Stream CsvStream) : IRequest<ImportResult>;
