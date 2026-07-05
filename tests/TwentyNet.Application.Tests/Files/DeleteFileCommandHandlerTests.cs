using NSubstitute;
using TwentyNet.Application.Files.DeleteFile;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence.Repositories;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Application.Tests.Files;

public sealed class DeleteFileCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldSoftDelete_AndRemoveFromStorage()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfRepository<FileEntity>(context);
        var storageDriver = Substitute.For<IStorageDriver>();
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
            Status = FileStatus.Uploaded
        };

        await repository.AddAsync(file);
        await repository.SaveChangesAsync();

        var handler = new DeleteFileCommandHandler(repository, storageDriver, authContext);

        // Act
        await handler.Handle(new DeleteFileCommand(file.Id), CancellationToken.None);

        // Assert
        var fileInDb = await context.Files.FindAsync(file.Id);
        Assert.NotNull(fileInDb);
        Assert.NotNull(fileInDb.DeletedAt);
        await storageDriver.Received(1).DeleteAsync(file.StorageKey, Arg.Any<CancellationToken>());
    }
}
