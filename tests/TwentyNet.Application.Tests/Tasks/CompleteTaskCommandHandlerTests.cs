using MediatR;
using NSubstitute;
using TwentyNet.Application.Tasks.CompleteTask;
using TwentyNet.Domain.Entities;
using TaskStatus = TwentyNet.Domain.Enums.TaskStatus;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;
using TaskItemEntity = TwentyNet.Domain.Entities.TaskItem;

namespace TwentyNet.Application.Tests.Tasks;

public sealed class CompleteTaskCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldMarkTaskAsDone_AndPublishEvent()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        context.Workspaces.Add(new Workspace { Id = workspaceId, Name = "Test" });
        context.Users.Add(new User { Id = userId, Email = new TwentyNet.Domain.ValueObjects.Email("user@test.com"), PasswordHash = "hash" });
        context.Companies.Add(new Company { Id = companyId, Name = "Acme", WorkspaceId = workspaceId });
        context.TaskItems.Add(new TaskItemEntity
        {
            Id = taskId,
            Title = "Follow up",
            Status = TaskStatus.Todo,
            WorkspaceId = workspaceId,
            CompanyId = companyId
        });
        await context.SaveChangesAsync();

        var publisher = Substitute.For<IPublisher>();
        var handler = CreateHandler(context, workspaceId, userId, publisher);

        // Act
        var result = await handler.Handle(new CompleteTaskCommand(taskId), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TaskStatus.Done, result.Status);

        var taskInDb = await context.TaskItems.FindAsync(taskId);
        Assert.NotNull(taskInDb);
        Assert.Equal(TaskStatus.Done, taskInDb.Status);

        await publisher.Received(1).Publish(
            Arg.Is<TaskCompletedEvent>(e =>
                e.WorkspaceId == workspaceId
                && e.TaskId == taskId
                && e.CompanyId == companyId
                && e.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    private static CompleteTaskCommandHandler CreateHandler(
        AppDbContext context,
        Guid workspaceId,
        Guid userId,
        IPublisher publisher)
    {
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);
        authContext.UserId.Returns(userId);

        return new CompleteTaskCommandHandler(
            new EfRepository<TaskItemEntity>(context),
            MapperTestHelper.CreateMapper(),
            authContext,
            publisher);
    }
}
