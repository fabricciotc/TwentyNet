using System.Security.Claims;

namespace TwentyNet.BFF.GraphQL.Extensions;

public static class GraphQLContextExtensions
{
    public static Guid? GetWorkspaceId(this ClaimsPrincipal? user)
    {
        var claim = user?.FindFirst("workspace_id");
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    public static Guid? GetUserId(this ClaimsPrincipal? user)
    {
        var claim = user?.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }
}
