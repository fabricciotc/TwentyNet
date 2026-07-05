using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.BFF.Services;

public sealed class CurrentUserService : IAuthContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId => GetGuidClaim("sub");
    public Guid? WorkspaceId => GetGuidClaim("workspace_id");
    public WorkspaceRole? Role => GetRoleClaim();
    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    private Guid? GetGuidClaim(string claimType)
    {
        var value = _httpContextAccessor.HttpContext?.User?.FindFirst(claimType)?.Value;
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private WorkspaceRole? GetRoleClaim()
    {
        var value = _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value;
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        return Enum.TryParse<WorkspaceRole>(value, out var role) ? role : null;
    }
}
