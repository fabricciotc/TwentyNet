using CsvHelper.Configuration.Attributes;

namespace TwentyNet.Application.ImportExport;

public sealed class CompanyImportRecord
{
    [Name("Name")]
    public string Name { get; set; } = string.Empty;

    [Name("DomainName")]
    [Optional]
    public string? DomainName { get; set; }

    [Name("Address")]
    [Optional]
    public string? Address { get; set; }
}
