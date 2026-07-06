using MediatR;

namespace TwentyNet.Application.ImportExport.ExportPeople;

public sealed record ExportPeopleQuery : IRequest<byte[]>;
