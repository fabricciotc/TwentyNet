using TwentyNet.Application.Files.AttachFileToCompany;
using TwentyNet.Application.Files.AttachFileToPerson;
using TwentyNet.Application.Files.CompleteFileUpload;
using TwentyNet.Application.Files.CreateFileUpload;
using TwentyNet.Application.Files.DeleteFile;
using TwentyNet.Application.Files.GetFileDownloadUrl;
using TwentyNet.Application.Files.UploadFileContent;
using TwentyNet.Domain.Enums;

namespace TwentyNet.Application.Tests.Files;

public sealed class FileValidatorsTests
{
    [Fact]
    public void CreateFileUploadValidator_ShouldPass_ForValidCommand()
    {
        var validator = new CreateFileUploadCommandValidator();
        var command = new CreateFileUploadCommand("file.pdf", "application/pdf", 1024, FileFolder.Attachment);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("", "application/pdf", 1)]
    [InlineData("file.pdf", "", 1)]
    [InlineData("file.pdf", "application/pdf", 0)]
    [InlineData("file.pdf", "application/pdf", -1)]
    public void CreateFileUploadValidator_ShouldFail_ForInvalidInputs(string name, string mimeType, long size)
    {
        var validator = new CreateFileUploadCommandValidator();
        var command = new CreateFileUploadCommand(name, mimeType, size, FileFolder.Attachment);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void CompleteFileUploadValidator_ShouldFail_WhenFileIdIsEmpty()
    {
        var validator = new CompleteFileUploadCommandValidator();
        var command = new CompleteFileUploadCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void DeleteFileValidator_ShouldFail_WhenFileIdIsEmpty()
    {
        var validator = new DeleteFileCommandValidator();
        var command = new DeleteFileCommand(Guid.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void GetFileDownloadUrlValidator_ShouldFail_WhenFileIdIsEmpty()
    {
        var validator = new GetFileDownloadUrlQueryValidator();
        var command = new GetFileDownloadUrlQuery(Guid.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void AttachFileToPersonValidator_ShouldFail_WhenIdsAreEmpty()
    {
        var validator = new AttachFileToPersonCommandValidator();
        var command = new AttachFileToPersonCommand(Guid.Empty, Guid.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void AttachFileToCompanyValidator_ShouldFail_WhenIdsAreEmpty()
    {
        var validator = new AttachFileToCompanyCommandValidator();
        var command = new AttachFileToCompanyCommand(Guid.Empty, Guid.Empty);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Fact]
    public void UploadFileContentValidator_ShouldFail_WhenContentIsNull()
    {
        var validator = new UploadFileContentCommandValidator();
        var command = new UploadFileContentCommand(Guid.NewGuid(), null!, "image/png");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
    }
}
