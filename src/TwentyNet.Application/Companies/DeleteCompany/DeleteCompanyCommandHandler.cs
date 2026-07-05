using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.DeleteCompany;

public sealed class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Unit>
{
    private readonly IRepository<Company> _repository;

    public DeleteCompanyCommandHandler(IRepository<Company> repository)
    {
        _repository = repository;
    }

    public async Task<Unit> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        await _repository.DeleteAsync(request.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
