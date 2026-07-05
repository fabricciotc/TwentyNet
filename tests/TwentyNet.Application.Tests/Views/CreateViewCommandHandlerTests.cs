using NSubstitute;
using TwentyNet.Application.Common;
using TwentyNet.Application.Views.CreateView;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Views;

public sealed class CreateViewCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreateView_WithFiltersAndSorts()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var repository = new EfRepository<View>(context);
        var mapper = MapperTestHelper.CreateMapper();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);
        var handler = new CreateViewCommandHandler(repository, mapper, authContext);

        var command = new CreateViewCommand(
            "Company",
            "My Company View",
            false,
            new List<FilterInput>
            {
                new("Name", FilterOperator.Contains, "Acme"),
                new("DomainName", FilterOperator.IsNotEmpty, null)
            },
            new List<SortInput>
            {
                new("Name", SortDirection.Asc)
            });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("My Company View", result.Name);
        Assert.Equal("Company", result.ObjectName);
        Assert.Equal(workspaceId, result.WorkspaceId);
        Assert.False(result.IsDefault);
        Assert.Equal(2, result.Filters.Count);
        Assert.Single(result.Sorts);

        var viewInDb = await context.Views.FindAsync(result.Id);
        Assert.NotNull(viewInDb);
        Assert.Equal("My Company View", viewInDb.Name);

        var filtersInDb = context.ViewFilters.Where(f => f.ViewId == result.Id).ToList();
        Assert.Equal(2, filtersInDb.Count);

        var sortsInDb = context.ViewSorts.Where(s => s.ViewId == result.Id).ToList();
        Assert.Single(sortsInDb);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWorkspaceIsMissing()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfRepository<View>(context);
        var mapper = MapperTestHelper.CreateMapper();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns((Guid?)null);
        var handler = new CreateViewCommandHandler(repository, mapper, authContext);

        var command = new CreateViewCommand(
            "Company",
            "View",
            false,
            new List<FilterInput>(),
            new List<SortInput>());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => handler.Handle(command, CancellationToken.None));
    }
}
