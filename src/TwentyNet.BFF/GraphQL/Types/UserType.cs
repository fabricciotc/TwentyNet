namespace TwentyNet.BFF.GraphQL.Types;

public sealed record UserType(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    bool IsEmailVerified,
    bool Disabled,
    DateTime CreatedAt,
    DateTime UpdatedAt);
