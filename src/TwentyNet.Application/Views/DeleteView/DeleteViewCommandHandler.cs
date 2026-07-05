using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Views.DeleteView;

public sealed class DeleteViewCommandHandler : IRequestHandler<DeleteViewCommand, Unit>
{
    private readonly IRepository<View> _viewRepository;
    private readonly IAuthContext _authContext;

    public DeleteViewCommandHandler(IRepository<View> viewRepository, IAuthContext authContext)
    {
        _viewRepository = viewRepository;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(DeleteViewCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;

        var view = (await _viewRepository.ListAsync(
            v => v.Id == request.Id && v.WorkspaceId == workspaceId,
            cancellationToken)).FirstOrDefault()
            ?? throw new KeyNotFoundException($"View with id {request.Id} not found.");

        await _viewRepository.DeleteAsync(view.Id, cancellationToken);
        await _viewRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
