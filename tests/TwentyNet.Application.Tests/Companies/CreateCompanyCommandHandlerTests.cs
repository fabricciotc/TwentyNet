using NSubstitute;
using TwentyNet.Application.Companies.CreateCompany;
using TwentyNet.Domain.Common;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Companies;

public sealed class CreateCompanyCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreateCompany_AndReturnDto()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfRepository<Company>(context);
        var mapper = MapperTestHelper.CreateMapper();
        var authContext = Substitute.For<IAuthContext>();
        var realTimeNotifier = Substitute.For<IRealTimeNotifier>();
        var workspaceId = Guid.NewGuid();
        authContext.WorkspaceId.Returns(workspaceId);
        var handler = new CreateCompanyCommandHandler(repository, mapper, authContext, realTimeNotifier);

        var command = new CreateCompanyCommand(
            "Twenty CRM",
            "twenty.com",
            "123 Main St");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Twenty CRM", result.Name);
        Assert.Equal("twenty.com", result.DomainName);
        Assert.Equal(workspaceId, result.WorkspaceId);
        Assert.NotEqual(Guid.Empty, result.Id);

        var companyInDb = await context.Companies.FindAsync(result.Id);
        Assert.NotNull(companyInDb);
        Assert.Equal("Twenty CRM", companyInDb.Name);
    }

    [Fact]
    public async Task Handle_ShouldNotifyRealTimeNotifier_WithObjectRecordCreatedEvent()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfRepository<Company>(context);
        var mapper = MapperTestHelper.CreateMapper();
        var authContext = Substitute.For<IAuthContext>();
        var realTimeNotifier = Substitute.For<IRealTimeNotifier>();
        var workspaceId = Guid.NewGuid();
        authContext.WorkspaceId.Returns(workspaceId);
        var handler = new CreateCompanyCommandHandler(repository, mapper, authContext, realTimeNotifier);

        var command = new CreateCompanyCommand(
            "Twenty CRM",
            "twenty.com",
            "123 Main St");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var call = Assert.Single(realTimeNotifier.ReceivedCalls()
            .Where(c => c.GetMethodInfo().Name == nameof(IRealTimeNotifier.NotifyAsync)));

        var args = call.GetArguments();
        Assert.IsType<ObjectRecordCreatedEvent>(args[0]);
        var createdEvent = (ObjectRecordCreatedEvent)args[0]!;
        Assert.Equal(workspaceId, createdEvent.WorkspaceId);
        Assert.Equal("Company", createdEvent.ObjectName);
        Assert.Equal(result.Id, createdEvent.RecordId);
    }
}
