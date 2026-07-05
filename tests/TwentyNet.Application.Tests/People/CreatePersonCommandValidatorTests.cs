using TwentyNet.Application.People.CreatePerson;

namespace TwentyNet.Application.Tests.People;

public sealed class CreatePersonCommandValidatorTests
{
    private readonly CreatePersonCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenEmailIsInvalid()
    {
        var command = new CreatePersonCommand(
            "John",
            "Doe",
            "not-an-email",
            null,
            null);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_ShouldPass_ForValidCommand()
    {
        var command = new CreatePersonCommand(
            "John",
            "Doe",
            "john@example.com",
            "+1 555 1234",
            null);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
