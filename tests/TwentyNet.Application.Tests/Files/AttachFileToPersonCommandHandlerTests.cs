using NSubstitute;
using TwentyNet.Application.Files.AttachFileToPerson;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence.Repositories;
using FileEntity = TwentyNet.Domain.Entities.File;

namespace TwentyNet.Application.Tests.Files;

public sealed class AttachFileToPersonCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldAssociateFileWithPerson()
    {
        // Arrange
        await using var context = CreateInMemoryContext();
        var fileRepository = new EfRepository<FileEntity>(context);
        var personRepository = new EfRepository<Domain.Entities.Person>(context);
        var authContext = Substitute.For<IAuthContext>();
        var workspaceId = Guid.NewGuid();
        authContext.WorkspaceId.Returns(workspaceId);

        var person = new Domain.Entities.Person
        {
            FirstName = "John",
            LastName = "Doe",
            WorkspaceId = workspaceId
        };

        var file = new FileEntity
        {
            Name = "avatar.png",
            MimeType = "image/png",
            Size = 100,
            Folder = FileFolder.Avatar,
            StorageKey = $"{workspaceId}/avatar/{Guid.NewGuid()}-avatar.png",
            WorkspaceId = workspaceId,
            Status = FileStatus.Uploaded
        };

        await personRepository.AddAsync(person);
        await fileRepository.AddAsync(file);
        await fileRepository.SaveChangesAsync();

        var handler = new AttachFileToPersonCommandHandler(fileRepository, personRepository, authContext);

        // Act
        await handler.Handle(new AttachFileToPersonCommand(person.Id, file.Id), CancellationToken.None);

        // Assert
        var fileInDb = await context.Files.FindAsync(file.Id);
        Assert.NotNull(fileInDb);
        Assert.Equal(person.Id, fileInDb.PersonId);
        Assert.Null(fileInDb.CompanyId);
    }
}
