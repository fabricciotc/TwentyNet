using TwentyNet.Application.Companies;
using TwentyNet.Application.People;
using TwentyNet.Application.Views;
using TwentyNet.BFF.GraphQL.Types;

namespace TwentyNet.BFF.GraphQL.Extensions;

public static class GraphQLMappingExtensions
{
    public static CompanyType ToGraphQL(this CompanyDto dto) => new(
        dto.Id,
        dto.Name,
        dto.DomainName,
        dto.Address,
        dto.WorkspaceId,
        dto.CreatedAt,
        dto.UpdatedAt);

    public static PersonType ToGraphQL(this PersonDto dto) => new(
        dto.Id,
        dto.FirstName,
        dto.LastName,
        dto.Email,
        dto.Phone,
        dto.CompanyId,
        dto.WorkspaceId,
        dto.CreatedAt,
        dto.UpdatedAt);

    public static ViewType ToGraphQL(this ViewDto dto) => new(
        dto.Id,
        dto.WorkspaceId,
        dto.ObjectName,
        dto.Name,
        dto.IsDefault,
        dto.Filters.Select(f => f.ToGraphQL()).ToList(),
        dto.Sorts.Select(s => s.ToGraphQL()).ToList(),
        dto.CreatedAt,
        dto.UpdatedAt);

    public static ViewFilterType ToGraphQL(this ViewFilterDto dto) => new(
        dto.Id,
        dto.Field,
        dto.Operator.ToString(),
        dto.Value);

    public static ViewSortType ToGraphQL(this ViewSortDto dto) => new(
        dto.Id,
        dto.Field,
        dto.Direction.ToString());
}
