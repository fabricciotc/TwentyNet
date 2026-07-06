using System.Text.Json;
using NSubstitute;
using TwentyNet.Application.Workflows.CreateWorkflow;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.Workflows;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Workflows;

public sealed class CreateWorkflowCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreateWorkflow_AndStoreActionsAsJson()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);
        var handler = new CreateWorkflowCommandHandler(
            new EfRepository<Workflow>(context),
            MapperTestHelper.CreateMapper(),
            authContext);

        var command = new CreateWorkflowCommand(
            "On company created",
            WorkflowTriggerType.RecordCreated,
            "Company",
            null,
            new List<WorkflowActionConfig>
            {
                new(WorkflowActionType.CreateTask, Task: new CreateTaskActionConfig("Follow up"))
            });

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.Equal("On company created", result.Name);
        Assert.Equal(workspaceId, result.WorkspaceId);
        Assert.Single(result.Actions);

        var workflowInDb = await context.Workflows.FindAsync(result.Id);
        Assert.NotNull(workflowInDb);
        Assert.Contains("Follow up", workflowInDb.Actions);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWorkspaceMissing()
    {
        await using var context = CreateInMemoryContext();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns((Guid?)null);
        var handler = new CreateWorkflowCommandHandler(
            new EfRepository<Workflow>(context),
            MapperTestHelper.CreateMapper(),
            authContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new CreateWorkflowCommand("x", WorkflowTriggerType.RecordCreated, null, null, Array.Empty<WorkflowActionConfig>()), CancellationToken.None));
    }
}
