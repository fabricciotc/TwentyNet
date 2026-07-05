using TwentyNet.Application.Companies.CreateCompany;

namespace TwentyNet.Application.Tests.Companies;

public sealed class CreateCompanyCommandValidatorTests
{
    private readonly CreateCompanyCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenNameIsEmpty()
    {
        var command = new CreateCompanyCommand(string.Empty, null, null);
        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var command = new CreateCompanyCommand("Twenty CRM", "twenty.com", "Address");
        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
