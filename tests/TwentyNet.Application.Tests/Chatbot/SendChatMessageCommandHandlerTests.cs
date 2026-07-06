using NSubstitute;
using TwentyNet.Application.Chatbot.CreateSession;
using TwentyNet.Application.Chatbot.SendMessage;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Chatbot;

public sealed class SendChatMessageCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldStoreUserMessage_AndAssistantReply()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);
        authContext.UserId.Returns(userId);

        context.ChatSessions.Add(new ChatSession
        {
            Title = "Test",
            WorkspaceId = workspaceId,
            UserId = userId
        });
        await context.SaveChangesAsync();

        var session = context.ChatSessions.Single();

        var chatbotProvider = Substitute.For<IChatbotProvider>();
        chatbotProvider.AskAsync(Arg.Any<IReadOnlyList<ChatMessageInput>>(), Arg.Any<CancellationToken>())
            .Returns("Assistant reply");

        var handler = new SendChatMessageCommandHandler(
            new EfRepository<ChatSession>(context),
            new EfRepository<ChatMessage>(context),
            chatbotProvider,
            MapperTestHelper.CreateMapper(),
            authContext);

        var result = await handler.Handle(new SendChatMessageCommand(session.Id, "Hello"), CancellationToken.None);

        Assert.Equal("assistant", result.Role);
        Assert.Equal("Assistant reply", result.Content);
        Assert.Equal(2, context.ChatMessages.Count());
    }

    [Fact]
    public async Task CreateSession_ShouldStoreSession()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);
        authContext.UserId.Returns(userId);

        var handler = new CreateChatSessionCommandHandler(
            new EfRepository<ChatSession>(context),
            MapperTestHelper.CreateMapper(),
            authContext);

        var result = await handler.Handle(new CreateChatSessionCommand("New session"), CancellationToken.None);

        Assert.Equal("New session", result.Title);
        Assert.Equal(workspaceId, result.WorkspaceId);
        Assert.Equal(userId, result.UserId);
        Assert.Single(context.ChatSessions);
    }
}
