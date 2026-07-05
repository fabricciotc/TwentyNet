using NSubstitute;
using TwentyNet.Application.Companies.ListCompanies;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
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

        var repository = new EfRepository<Company>(context);
        var mapper = MapperTestHelper.CreateMapper();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceA);
        var handler = new ListCompaniesQueryHandler(repository, mapper, authContext);

        // Act
        var result = await handler.Handle(new ListCompaniesQuery(), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, dto => Assert.Equal(workspaceA, dto.WorkspaceId));
    }
}
