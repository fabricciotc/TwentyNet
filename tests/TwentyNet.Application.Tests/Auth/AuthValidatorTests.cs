using TwentyNet.Application.Auth.LoginUser;
using TwentyNet.Application.Auth.LogoutUser;
using TwentyNet.Application.Auth.RotateToken;
using TwentyNet.Application.Auth.RegisterUser;

namespace TwentyNet.Application.Tests.Auth;

public sealed class AuthValidatorTests
{
    [Fact]
    public void RegisterUserCommandValidator_ShouldFail_WhenEmailIsInvalid()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand("invalid-email", "Password123!", "John", "Doe", "Workspace");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void RegisterUserCommandValidator_ShouldFail_WhenPasswordIsTooShort()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand("user@example.com", "short", "John", "Doe", "Workspace");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void RegisterUserCommandValidator_ShouldPass_ForValidCommand()
    {
        var validator = new RegisterUserCommandValidator();
        var command = new RegisterUserCommand("user@example.com", "Password123!", "John", "Doe", "Workspace");

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void LoginUserCommandValidator_ShouldFail_WhenWorkspaceIdIsEmpty()
    {
        var validator = new LoginUserCommandValidator();
        var command = new LoginUserCommand("user@example.com", "password", Guid.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "WorkspaceId");
    }

    [Fact]
    public void LoginUserCommandValidator_ShouldPass_ForValidCommand()
    {
        var validator = new LoginUserCommandValidator();
        var command = new LoginUserCommand("user@example.com", "password", Guid.NewGuid());

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RefreshTokenCommandValidator_ShouldFail_WhenTokenIsEmpty()
    {
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand(string.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RefreshToken");
    }

    [Fact]
    public void LogoutUserCommandValidator_ShouldFail_WhenTokenIsEmpty()
    {
        var validator = new LogoutUserCommandValidator();
        var command = new LogoutUserCommand(string.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "RefreshToken");
    }
}
