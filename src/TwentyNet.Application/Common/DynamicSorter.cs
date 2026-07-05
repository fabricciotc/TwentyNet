using System.Linq.Expressions;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Common;

public static class DynamicSorter
{
    public static IQueryable<Company> ApplySorts(IQueryable<Company> query, IEnumerable<SortInput> sorts)
    {
        IOrderedQueryable<Company>? ordered = null;
        foreach (var sort in sorts)
        {
            ordered = sort.Field switch
            {
                "Name" => ApplySort(query, ordered, c => c.Name, sort.Direction),
                "DomainName" => ApplySort(query, ordered, c => c.DomainName, sort.Direction),
                "Address" => ApplySort(query, ordered, c => c.Address, sort.Direction),
                "CreatedAt" => ApplySort(query, ordered, c => c.CreatedAt, sort.Direction),
                "UpdatedAt" => ApplySort(query, ordered, c => c.UpdatedAt, sort.Direction),
                _ => ordered
            };
        }

        return ordered ?? query.OrderByDescending(c => c.UpdatedAt);
    }

    public static IQueryable<Person> ApplySorts(IQueryable<Person> query, IEnumerable<SortInput> sorts)
    {
        IOrderedQueryable<Person>? ordered = null;
        foreach (var sort in sorts)
        {
            ordered = sort.Field switch
            {
                "FirstName" => ApplySort(query, ordered, p => p.FirstName, sort.Direction),
                "LastName" => ApplySort(query, ordered, p => p.LastName, sort.Direction),
                "Email" => ApplyValueObjectSort(query, ordered, p => p.Email, sort.Direction),
                "Phone" => ApplyValueObjectSort(query, ordered, p => p.Phone, sort.Direction),
                "CreatedAt" => ApplySort(query, ordered, p => p.CreatedAt, sort.Direction),
                "UpdatedAt" => ApplySort(query, ordered, p => p.UpdatedAt, sort.Direction),
                _ => ordered
            };
        }

        return ordered ?? query.OrderByDescending(p => p.UpdatedAt);
    }

    private static IOrderedQueryable<T> ApplySort<T, TKey>(
        IQueryable<T> query,
        IOrderedQueryable<T>? ordered,
        Expression<Func<T, TKey>> selector,
        SortDirection direction)
    {
        if (ordered is null)
        {
            return direction == SortDirection.Asc
                ? query.OrderBy(selector)
                : query.OrderByDescending(selector);
        }

        return direction == SortDirection.Asc
            ? ordered.ThenBy(selector)
            : ordered.ThenByDescending(selector);
    }

    private static IOrderedQueryable<Person> ApplyValueObjectSort<TValueObject>(
        IQueryable<Person> query,
        IOrderedQueryable<Person>? ordered,
        Expression<Func<Person, TValueObject?>> selector,
        SortDirection direction)
        where TValueObject : class
    {
        var parameter = selector.Parameters[0];
        var memberAccess = selector.Body;
        var valueProperty = typeof(TValueObject).GetProperty("Value")!;
        var valueAccess = Expression.MakeMemberAccess(memberAccess, valueProperty);
        var nullCheck = Expression.Equal(memberAccess, Expression.Constant(null, typeof(TValueObject)));
        var safeValueAccess = Expression.Condition(
            nullCheck,
            Expression.Constant(null, typeof(string)),
            valueAccess);
        var valueLambda = Expression.Lambda<Func<Person, object?>>(Expression.Convert(safeValueAccess, typeof(object)), parameter);
        return ApplySort(query, ordered, valueLambda, direction);
    }
}
