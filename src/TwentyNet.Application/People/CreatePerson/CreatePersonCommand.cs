using MediatR;

namespace TwentyNet.Application.People.CreatePerson;

public sealed record CreatePersonCommand(
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    Guid? CompanyId) : IRequest<PersonDto>;
