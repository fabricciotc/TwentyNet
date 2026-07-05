using NSubstitute;
using TwentyNet.Application.Files.CreateFileUpload;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence.Repositories;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Application.Tests.Files;

public sealed class CreateFileUploadCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreatePendingFile_AndReturnUploadResponse()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfRepository<FileEntity>(context);
        var storageDriver = Substitute.For<IStorageDriver>();
        var authContext = Substitute.For<IAuthContext>();
        var workspaceId = Guid.NewGuid();
        authContext.WorkspaceId.Returns(workspaceId);

        storageDriver.GetPresignedUploadUrlAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns((string?)null);

        var handler = new CreateFileUploadCommandHandler(repository, storageDriver, authContext);
        var command = new CreateFileUploadCommand("report.pdf", "application/pdf", 1024, FileFolder.Attachment);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.FileId);
        Assert.Contains("/api/files/", result.UploadUrl);
        Assert.Contains(workspaceId.ToString(), result.StorageKey);

        var fileInDb = await context.Files.FindAsync(result.FileId);
        Assert.NotNull(fileInDb);
        Assert.Equal(FileStatus.Pending, fileInDb.Status);
        Assert.Equal(workspaceId, fileInDb.WorkspaceId);
        Assert.Equal("report.pdf", fileInDb.Name);
    }

    [Fact]
    public async Task Handle_ShouldReturnPresignedUrl_WhenDriverProvidesOne()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var repository = new EfRepository<FileEntity>(context);
        var storageDriver = Substitute.For<IStorageDriver>();
        var authContext = Substitute.For<IAuthContext>();
        var workspaceId = Guid.NewGuid();
        authContext.WorkspaceId.Returns(workspaceId);

        storageDriver.GetPresignedUploadUrlAsync(Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns("https://s3.example.com/presigned");

        var handler = new CreateFileUploadCommandHandler(repository, storageDriver, authContext);
        var command = new CreateFileUploadCommand("report.pdf", "application/pdf", 1024, FileFolder.Attachment);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal("https://s3.example.com/presigned", result.UploadUrl);
    }
}
