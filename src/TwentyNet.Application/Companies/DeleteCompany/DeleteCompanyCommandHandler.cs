using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Companies.DeleteCompany;

public sealed class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Unit>
{
    private readonly IRepository<Company> _repository;
    private readonly IAuthContext _authContext;
    private readonly IRealTimeNotifier _realTimeNotifier;

    public DeleteCompanyCommandHandler(
        IRepository<Company> repository,
        IAuthContext authContext,
        IRealTimeNotifier realTimeNotifier)
    {
        _repository = repository;
        _authContext = authContext;
        _realTimeNotifier = realTimeNotifier;
    }

    public async Task<Unit> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var companies = await _repository.ListAsync(
            c => c.Id == request.Id && c.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var company = companies.FirstOrDefault();

        if (company is null)
        {
            throw new KeyNotFoundException($"Company with id {request.Id} not found.");
        }

        await _repository.DeleteAsync(request.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        await _realTimeNotifier.NotifyAsync(
            new ObjectRecordDeletedEvent(_authContext.WorkspaceId.Value, "Company", request.Id),
            cancellationToken);

        return Unit.Value;
    }
}
