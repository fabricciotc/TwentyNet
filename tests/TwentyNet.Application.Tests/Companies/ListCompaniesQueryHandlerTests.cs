using NSubstitute;
using TwentyNet.Application.Common;
using TwentyNet.Application.Companies.ListCompanies;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Companies;

public sealed class ListCompaniesQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnOnlyCompaniesForWorkspace()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceA = Guid.NewGuid();
        var workspaceB = Guid.NewGuid();

        context.Companies.AddRange(
            new Company { Name = "Company A1", WorkspaceId = workspaceA },
            new Company { Name = "Company A2", WorkspaceId = workspaceA },
            new Company { Name = "Company B1", WorkspaceId = workspaceB });

        await context.SaveChangesAsync();

        var handler = CreateHandler(context, workspaceA);

        // Act
        var result = await handler.Handle(new ListCompaniesQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, dto => Assert.Equal(workspaceA, dto.WorkspaceId));
    }

    [Fact]
    public async Task Handle_ShouldFilterByNameEquals()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();

        context.Companies.AddRange(
            new Company { Name = "Acme", WorkspaceId = workspaceId },
            new Company { Name = "Globex", WorkspaceId = workspaceId });

        await context.SaveChangesAsync();

        var handler = CreateHandler(context, workspaceId);
        var query = new ListCompaniesQuery(
            Filters: new List<FilterInput> { new("Name", FilterOperator.Equals, "Acme") });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Acme", result.Items[0].Name);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task Handle_ShouldFilterByDomainNameContains()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();

        context.Companies.AddRange(
            new Company { Name = "Acme", DomainName = "acme.com", WorkspaceId = workspaceId },
            new Company { Name = "Globex", DomainName = "globex.org", WorkspaceId = workspaceId });

        await context.SaveChangesAsync();

        var handler = CreateHandler(context, workspaceId);
        var query = new ListCompaniesQuery(
            Filters: new List<FilterInput> { new("DomainName", FilterOperator.Contains, "acme") });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Acme", result.Items[0].Name);
    }

    [Fact]
    public async Task Handle_ShouldSortByNameAscending()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();

        context.Companies.AddRange(
            new Company { Name = "Zebra", WorkspaceId = workspaceId },
            new Company { Name = "Alpha", WorkspaceId = workspaceId },
            new Company { Name = "Beta", WorkspaceId = workspaceId });

        await context.SaveChangesAsync();

        var handler = CreateHandler(context, workspaceId);
        var query = new ListCompaniesQuery(
            Sorts: new List<SortInput> { new("Name", SortDirection.Asc) });

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(new[] { "Alpha", "Beta", "Zebra" }, result.Items.Select(c => c.Name).ToArray());
    }

    [Fact]
    public async Task Handle_ShouldApplyPagination()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();

        context.Companies.AddRange(
            new Company { Name = "A", WorkspaceId = workspaceId },
            new Company { Name = "B", WorkspaceId = workspaceId },
            new Company { Name = "C", WorkspaceId = workspaceId },
            new Company { Name = "D", WorkspaceId = workspaceId });

        await context.SaveChangesAsync();

        var handler = CreateHandler(context, workspaceId);
        var query = new ListCompaniesQuery(
            Sorts: new List<SortInput> { new("Name", SortDirection.Asc) },
            Skip: 1,
            Take: 2);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(4, result.TotalCount);
        Assert.Equal("B", result.Items[0].Name);
        Assert.Equal("C", result.Items[1].Name);
        Assert.Equal(1, result.Skip);
        Assert.Equal(2, result.Take);
    }

    [Fact]
    public async Task Handle_ShouldSearchAcrossMultipleFields()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();

        context.Companies.AddRange(
            new Company { Name = "Acme Corp", WorkspaceId = workspaceId },
            new Company { Name = "Globex", DomainName = "acme-globals.com", WorkspaceId = workspaceId },
            new Company { Name = "Soylent", Address = "123 Acme Street", WorkspaceId = workspaceId },
            new Company { Name = "Initech", WorkspaceId = workspaceId });

        await context.SaveChangesAsync();

        var handler = CreateHandler(context, workspaceId);
        var query = new ListCompaniesQuery(Search: "acme");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
    }

    private static ListCompaniesQueryHandler CreateHandler(AppDbContext context, Guid workspaceId)
    {
        var companyRepository = new EfRepository<Company>(context);
        var viewRepository = new EfRepository<View>(context);
        var filterRepository = new EfRepository<ViewFilter>(context);
        var sortRepository = new EfRepository<ViewSort>(context);
        var mapper = MapperTestHelper.CreateMapper();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);

        return new ListCompaniesQueryHandler(
            companyRepository,
            viewRepository,
            filterRepository,
            sortRepository,
            mapper,
            authContext);
    }
}
