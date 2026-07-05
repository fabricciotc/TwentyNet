using MediatR;

namespace TwentyNet.Application.People.UpdatePerson;

public sealed record UpdatePersonCommand(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? Phone,
    Guid? CompanyId) : IRequest<PersonDto>;
