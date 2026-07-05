using System.Linq.Expressions;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Common;

public static class DynamicFilterApplicator
{
    public static IQueryable<Company> ApplyFilters(IQueryable<Company> query, IEnumerable<FilterInput> filters)
    {
        foreach (var filter in filters)
        {
            query = filter.Field switch
            {
                "Name" => ApplyStringFilter(query, c => c.Name, filter),
                "DomainName" => ApplyStringFilter(query, c => c.DomainName, filter),
                "Address" => ApplyStringFilter(query, c => c.Address, filter),
                _ => query
            };
        }

        return query;
    }

    public static IQueryable<Person> ApplyFilters(IQueryable<Person> query, IEnumerable<FilterInput> filters)
    {
        foreach (var filter in filters)
        {
            query = filter.Field switch
            {
                "FirstName" => ApplyStringFilter(query, p => p.FirstName, filter),
                "LastName" => ApplyStringFilter(query, p => p.LastName, filter),
                "Email" => ApplyValueObjectFilter(query, p => p.Email, filter),
                "Phone" => ApplyValueObjectFilter(query, p => p.Phone, filter),
                _ => query
            };
        }

        return query;
    }

    public static IQueryable<Company> ApplySearch(IQueryable<Company> query, string search)
    {
        var lowered = search.ToLowerInvariant();
        return query.Where(c =>
            c.Name.ToLower().Contains(lowered) ||
            (c.DomainName != null && c.DomainName.ToLower().Contains(lowered)) ||
            (c.Address != null && c.Address.ToLower().Contains(lowered)));
    }

    public static IQueryable<Person> ApplySearch(IQueryable<Person> query, string search)
    {
        var lowered = search.ToLowerInvariant();
        return query.Where(p =>
            p.FirstName.ToLower().Contains(lowered) ||
            p.LastName.ToLower().Contains(lowered) ||
            (p.Email != null && p.Email.Value.ToLower().Contains(lowered)) ||
            (p.Phone != null && p.Phone.Value.ToLower().Contains(lowered)));
    }

    private static IQueryable<T> ApplyStringFilter<T>(IQueryable<T> query, Expression<Func<T, string?>> selector, FilterInput filter)
    {
        var predicate = BuildStringPredicate(selector, filter);
        return query.Where(predicate);
    }

    private static IQueryable<Person> ApplyValueObjectFilter<TValueObject>(
        IQueryable<Person> query,
        Expression<Func<Person, TValueObject?>> selector,
        FilterInput filter)
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

        var valueLambda = Expression.Lambda<Func<Person, string?>>(safeValueAccess, parameter);
        return ApplyStringFilter(query, valueLambda, filter);
    }

    private static Expression<Func<T, bool>> BuildStringPredicate<T>(Expression<Func<T, string?>> selector, FilterInput filter)
    {
        var parameter = selector.Parameters[0];
        var member = selector.Body;

        return filter.Operator switch
        {
            FilterOperator.Equals => BuildEqualsPredicate<T>(parameter, member, filter.Value),
            FilterOperator.Contains => BuildContainsPredicate<T>(parameter, member, filter.Value),
            FilterOperator.IsEmpty => BuildIsEmptyPredicate<T>(parameter, member),
            FilterOperator.IsNotEmpty => BuildIsNotEmptyPredicate<T>(parameter, member),
            FilterOperator.GreaterThan => BuildComparisonPredicate<T>(parameter, member, filter.Value, ExpressionType.GreaterThan),
            FilterOperator.LessThan => BuildComparisonPredicate<T>(parameter, member, filter.Value, ExpressionType.LessThan),
            _ => _ => true
        };
    }

    private static Expression<Func<T, bool>> BuildEqualsPredicate<T>(ParameterExpression parameter, Expression member, string? value)
    {
        var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
        var comparison = Expression.Equal(member, Expression.Constant(value ?? string.Empty, typeof(string)));
        var body = Expression.AndAlso(notNull, comparison);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression<Func<T, bool>> BuildContainsPredicate<T>(ParameterExpression parameter, Expression member, string? value)
    {
        var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
        var method = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
        var call = Expression.Call(member, method, Expression.Constant(value ?? string.Empty, typeof(string)));
        var body = Expression.AndAlso(notNull, call);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression<Func<T, bool>> BuildIsEmptyPredicate<T>(ParameterExpression parameter, Expression member)
    {
        var method = typeof(string).GetMethod(nameof(string.IsNullOrEmpty), new[] { typeof(string) })!;
        var call = Expression.Call(method, member);
        return Expression.Lambda<Func<T, bool>>(call, parameter);
    }

    private static Expression<Func<T, bool>> BuildIsNotEmptyPredicate<T>(ParameterExpression parameter, Expression member)
    {
        var method = typeof(string).GetMethod(nameof(string.IsNullOrEmpty), new[] { typeof(string) })!;
        var call = Expression.Call(method, member);
        var body = Expression.Not(call);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static Expression<Func<T, bool>> BuildComparisonPredicate<T>(
        ParameterExpression parameter,
        Expression member,
        string? value,
        ExpressionType comparison)
    {
        var notNull = Expression.NotEqual(member, Expression.Constant(null, typeof(string)));
        var compareMethod = typeof(string).GetMethod(nameof(string.Compare), new[] { typeof(string), typeof(string), typeof(StringComparison) })!;
        var compareCall = Expression.Call(compareMethod, member, Expression.Constant(value ?? string.Empty, typeof(string)), Expression.Constant(StringComparison.Ordinal));
        var comparisonExpr = comparison switch
        {
            ExpressionType.GreaterThan => Expression.GreaterThan(compareCall, Expression.Constant(0)),
            ExpressionType.LessThan => Expression.LessThan(compareCall, Expression.Constant(0)),
            _ => throw new NotSupportedException($"Comparison {comparison} is not supported.")
        };
        var body = Expression.AndAlso(notNull, comparisonExpr);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
}
