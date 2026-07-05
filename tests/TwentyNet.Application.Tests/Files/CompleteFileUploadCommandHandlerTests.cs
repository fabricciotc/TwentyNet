using NSubstitute;
using TwentyNet.Application.Files.CompleteFileUpload;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence.Repositories;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Application.Tests.Files;

public sealed class CompleteFileUploadCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldMarkFileAsUploaded()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfRepository<FileEntity>(context);
        var authContext = Substitute.For<IAuthContext>();
        var workspaceId = Guid.NewGuid();
        authContext.WorkspaceId.Returns(workspaceId);

        var file = new FileEntity
        {
            Name = "doc.pdf",
            MimeType = "application/pdf",
            Size = 100,
            Folder = FileFolder.Attachment,
            StorageKey = $"{workspaceId}/attachment/{Guid.NewGuid()}-doc.pdf",
            WorkspaceId = workspaceId,
            Status = FileStatus.Pending
        };

        await repository.AddAsync(file);
        await repository.SaveChangesAsync();

        var handler = new CompleteFileUploadCommandHandler(repository, authContext);

        // Act
        await handler.Handle(new CompleteFileUploadCommand(file.Id), CancellationToken.None);

        // Assert
        var fileInDb = await context.Files.FindAsync(file.Id);
        Assert.NotNull(fileInDb);
        Assert.Equal(FileStatus.Uploaded, fileInDb.Status);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenFileNotFound()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfRepository<FileEntity>(context);
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(Guid.NewGuid());

        var handler = new CompleteFileUploadCommandHandler(repository, authContext);

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            handler.Handle(new CompleteFileUploadCommand(Guid.NewGuid()), CancellationToken.None));
    }
}
