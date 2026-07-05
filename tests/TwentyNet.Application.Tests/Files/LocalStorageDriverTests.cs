using Microsoft.Extensions.Options;
using TwentyNet.BFF.Options;
using TwentyNet.BFF.Services;

namespace TwentyNet.Application.Tests.Files;

public sealed class LocalStorageDriverTests : IDisposable
{
    private readonly string _tempPath;

    public LocalStorageDriverTests()
    {
        _tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempPath);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, recursive: true);
        }
    }

    private LocalStorageDriver CreateDriver()
    {
        var options = Options.Create(new StorageOptions { LocalPath = _tempPath });
        return new LocalStorageDriver(options);
    }

    [Fact]
    public async Task WriteAsync_ShouldCreateFile()
    {
        var driver = CreateDriver();
        var key = "workspace/attachment/test.txt";
        var content = "hello world"u8.ToArray();

        await using var stream = new MemoryStream(content);
        await driver.WriteAsync(key, stream, "text/plain");

        var fullPath = System.IO.Path.Combine(_tempPath, key);
        Assert.True(System.IO.File.Exists(fullPath));
        Assert.Equal(content, await System.IO.File.ReadAllBytesAsync(fullPath));
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnFileContent()
    {
        var driver = CreateDriver();
        var key = "workspace/attachment/test.txt";
        var content = "hello world"u8.ToArray();
        var fullPath = System.IO.Path.Combine(_tempPath, key);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath)!);
        await System.IO.File.WriteAllBytesAsync(fullPath, content);

        await using var stream = await driver.ReadAsync(key);
        using var reader = new StreamReader(stream);
        var result = await reader.ReadToEndAsync();

        Assert.Equal("hello world", result);
    }

    [Fact]
    public async Task ReadAsync_ShouldThrow_WhenFileDoesNotExist()
    {
        var driver = CreateDriver();
        await Assert.ThrowsAsync<FileNotFoundException>(() => driver.ReadAsync("missing.txt"));
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveFile()
    {
        var driver = CreateDriver();
        var key = "workspace/attachment/test.txt";
        var fullPath = System.IO.Path.Combine(_tempPath, key);
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(fullPath)!);
        await System.IO.File.WriteAllTextAsync(fullPath, "content");

        await driver.DeleteAsync(key);

        Assert.False(System.IO.File.Exists(fullPath));
    }

    [Fact]
    public async Task GetPresignedUrls_ShouldReturnNull()
    {
        var driver = CreateDriver();

        var uploadUrl = await driver.GetPresignedUploadUrlAsync("key", TimeSpan.FromMinutes(5));
        var downloadUrl = await driver.GetPresignedDownloadUrlAsync("key", TimeSpan.FromMinutes(5));

        Assert.Null(uploadUrl);
        Assert.Null(downloadUrl);
    }
}
