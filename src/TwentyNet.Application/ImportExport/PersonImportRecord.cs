using CsvHelper.Configuration.Attributes;

namespace TwentyNet.Application.ImportExport;

public sealed class PersonImportRecord
{
    [Name("FirstName")]
    public string FirstName { get; set; } = string.Empty;

    [Name("LastName")]
    public string LastName { get; set; } = string.Empty;

    [Name("Email")]
    public string Email { get; set; } = string.Empty;

    [Name("Phone")]
    [Optional]
    public string? Phone { get; set; }

    [Name("CompanyName")]
    [Optional]
    public string? CompanyName { get; set; }
}
