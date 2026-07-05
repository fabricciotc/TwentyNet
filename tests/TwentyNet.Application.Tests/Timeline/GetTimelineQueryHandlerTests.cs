using NSubstitute;
using TwentyNet.Application.Timeline.GetTimeline;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Timeline;

public sealed class GetTimelineQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldReturnActivitiesOrderedByCreatedAtDescending()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var otherWorkspaceId = Guid.NewGuid();
        var recordId = Guid.NewGuid();

        context.Workspaces.Add(new Workspace { Id = workspaceId, Name = "Test" });
        context.Workspaces.Add(new Workspace { Id = otherWorkspaceId, Name = "Other" });
        await context.SaveChangesAsync();

        context.TimelineActivities.AddRange(
            new TimelineActivity
            {
                WorkspaceId = workspaceId,
                ObjectName = "Company",
                RecordId = recordId,
                ActivityType = ActivityType.RecordCreated,
                Title = "Company created",
                CreatedAt = DateTime.UtcNow.AddHours(-2)
            },
            new TimelineActivity
            {
                WorkspaceId = workspaceId,
                ObjectName = "Company",
                RecordId = recordId,
                ActivityType = ActivityType.NoteCreated,
                Title = "Note created",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            new TimelineActivity
            {
                WorkspaceId = workspaceId,
                ObjectName = "Company",
                RecordId = recordId,
                ActivityType = ActivityType.TaskCreated,
                Title = "Task created",
                CreatedAt = DateTime.UtcNow
            },
            new TimelineActivity
            {
                WorkspaceId = otherWorkspaceId,
                ObjectName = "Company",
                RecordId = recordId,
                ActivityType = ActivityType.RecordCreated,
                Title = "Other workspace activity",
                CreatedAt = DateTime.UtcNow
            });

        await context.SaveChangesAsync();

        var handler = CreateHandler(context, workspaceId);
        var query = new GetTimelineQuery("Company", recordId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(ActivityType.TaskCreated, result[0].ActivityType);
        Assert.Equal(ActivityType.NoteCreated, result[1].ActivityType);
        Assert.Equal(ActivityType.RecordCreated, result[2].ActivityType);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWorkspaceIsMissing()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var handler = CreateHandler(context, null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            handler.Handle(new GetTimelineQuery("Company", Guid.NewGuid()), CancellationToken.None));
    }

    private static GetTimelineQueryHandler CreateHandler(AppDbContext context, Guid? workspaceId)
    {
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);

        return new GetTimelineQueryHandler(
            new EfRepository<TimelineActivity>(context),
            MapperTestHelper.CreateMapper(),
            authContext);
    }
}
