using System.Text;
using NSubstitute;
using TwentyNet.Application.ImportExport.ExportCompanies;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.ImportExport;

public sealed class ExportCompaniesQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldExportCompaniesInWorkspace()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        context.Companies.Add(new Company
        {
            Name = "Acme Inc",
            DomainName = "acme.com",
            Address = "123 Main St",
            WorkspaceId = workspaceId
        });
        context.Companies.Add(new Company
        {
            Name = "Other",
            WorkspaceId = Guid.NewGuid()
        });
        await context.SaveChangesAsync();

        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);

        var handler = new ExportCompaniesQueryHandler(new EfRepository<Company>(context), authContext);

        var bytes = await handler.Handle(new ExportCompaniesQuery(), CancellationToken.None);
        var csv = Encoding.UTF8.GetString(bytes);

        Assert.Contains("Acme Inc", csv);
        Assert.Contains("acme.com", csv);
        Assert.DoesNotContain("Other", csv);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWorkspaceMissing()
    {
        await using var context = CreateInMemoryContext();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns((Guid?)null);

        var handler = new ExportCompaniesQueryHandler(new EfRepository<Company>(context), authContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new ExportCompaniesQuery(), CancellationToken.None));
    }
}
