using MediatR;

namespace TwentyNet.Application.ImportExport.ExportCompanies;

public sealed record ExportCompaniesQuery : IRequest<byte[]>;
