using MediatR;
using NSubstitute;
using TwentyNet.Application.Tasks.CreateTask;
using TwentyNet.Domain.Entities;
using TaskStatus = TwentyNet.Domain.Enums.TaskStatus;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Tasks;

public sealed class CreateTaskCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreateTask_ForCompany_AndReturnDto()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        context.Workspaces.Add(new Workspace { Id = workspaceId, Name = "Test" });
        context.Users.Add(new User { Id = userId, Email = new TwentyNet.Domain.ValueObjects.Email("user@test.com"), PasswordHash = "hash" });
        context.Companies.Add(new Company { Id = companyId, Name = "Acme", WorkspaceId = workspaceId });
        await context.SaveChangesAsync();

        var handler = CreateHandler(context, workspaceId, userId);
        var command = new CreateTaskCommand("Follow up", null, null, companyId, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Follow up", result.Title);
        Assert.Equal(TaskStatus.Todo, result.Status);
        Assert.Equal(workspaceId, result.WorkspaceId);
        Assert.Equal(companyId, result.CompanyId);

        var taskInDb = await context.TaskItems.FindAsync(result.Id);
        Assert.NotNull(taskInDb);
    }

    [Fact]
    public async Task Handle_ShouldPublishTaskCreatedEvent()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var personId = Guid.NewGuid();

        context.Workspaces.Add(new Workspace { Id = workspaceId, Name = "Test" });
        context.Users.Add(new User { Id = userId, Email = new TwentyNet.Domain.ValueObjects.Email("user@test.com"), PasswordHash = "hash" });
        context.People.Add(new Person { Id = personId, FirstName = "John", LastName = "Doe", WorkspaceId = workspaceId });
        await context.SaveChangesAsync();

        var publisher = Substitute.For<IPublisher>();
        var handler = CreateHandler(context, workspaceId, userId, publisher);
        var command = new CreateTaskCommand("Send email", null, null, null, personId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await publisher.Received(1).Publish(
            Arg.Is<TaskCreatedEvent>(e =>
                e.WorkspaceId == workspaceId
                && e.TaskId == result.Id
                && e.PersonId == personId
                && e.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    private static CreateTaskCommandHandler CreateHandler(
        AppDbContext context,
        Guid workspaceId,
        Guid userId,
        IPublisher? publisher = null)
    {
        publisher ??= Substitute.For<IPublisher>();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);
        authContext.UserId.Returns(userId);

        return new CreateTaskCommandHandler(
            new EfRepository<TaskItem>(context),
            new EfRepository<Company>(context),
            new EfRepository<Person>(context),
            MapperTestHelper.CreateMapper(),
            authContext,
            publisher);
    }
}
