using MediatR;
using NSubstitute;
using TwentyNet.Application.Notes.CreateNote;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Notes;

public sealed class CreateNoteCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreateNote_ForCompany_AndReturnDto()
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
        var command = new CreateNoteCommand("Meeting notes", "Discussed pricing", companyId, null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Meeting notes", result.Title);
        Assert.Equal("Discussed pricing", result.Content);
        Assert.Equal(workspaceId, result.WorkspaceId);
        Assert.Equal(userId, result.CreatedByUserId);
        Assert.Equal(companyId, result.CompanyId);

        var noteInDb = await context.Notes.FindAsync(result.Id);
        Assert.NotNull(noteInDb);
    }

    [Fact]
    public async Task Handle_ShouldPublishNoteCreatedEvent()
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
        var command = new CreateNoteCommand("Call notes", "Called John", null, personId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await publisher.Received(1).Publish(
            Arg.Is<NoteCreatedEvent>(e =>
                e.WorkspaceId == workspaceId
                && e.NoteId == result.Id
                && e.PersonId == personId
                && e.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenCompanyDoesNotExist()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        context.Workspaces.Add(new Workspace { Id = workspaceId, Name = "Test" });
        context.Users.Add(new User { Id = userId, Email = new TwentyNet.Domain.ValueObjects.Email("user@test.com"), PasswordHash = "hash" });
        await context.SaveChangesAsync();

        var handler = CreateHandler(context, workspaceId, userId);
        var command = new CreateNoteCommand("Notes", "Content", Guid.NewGuid(), null);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    private static CreateNoteCommandHandler CreateHandler(
        AppDbContext context,
        Guid workspaceId,
        Guid userId,
        IPublisher? publisher = null)
    {
        publisher ??= Substitute.For<IPublisher>();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);
        authContext.UserId.Returns(userId);

        return new CreateNoteCommandHandler(
            new EfRepository<Note>(context),
            new EfRepository<Company>(context),
            new EfRepository<Person>(context),
            MapperTestHelper.CreateMapper(),
            authContext,
            publisher);
    }
}
