using Microsoft.EntityFrameworkCore;
using TwentyNet.Persistence;

namespace TwentyNet.Application.Tests;

public abstract class TestBase : IDisposable
{
    protected AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
