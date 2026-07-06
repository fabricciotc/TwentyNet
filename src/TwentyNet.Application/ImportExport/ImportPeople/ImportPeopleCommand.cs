using MediatR;

namespace TwentyNet.Application.ImportExport.ImportPeople;

public sealed record ImportPeopleCommand(Stream CsvStream) : IRequest<ImportResult>;
