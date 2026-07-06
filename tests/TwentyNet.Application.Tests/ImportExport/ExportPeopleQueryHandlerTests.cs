using System.Text;
using NSubstitute;
using TwentyNet.Application.ImportExport.ExportPeople;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.ValueObjects;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.ImportExport;

public sealed class ExportPeopleQueryHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldExportPeopleInWorkspace_WithCompanyName()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var company = new Company { Name = "Acme Inc", WorkspaceId = workspaceId };
        context.Companies.Add(company);
        context.People.Add(new Person
        {
            FirstName = "John",
            LastName = "Doe",
            Email = new Email("john@acme.com"),
            Phone = new PhoneNumber("+1234567890"),
            CompanyId = company.Id,
            WorkspaceId = workspaceId
        });
        await context.SaveChangesAsync();

        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);

        var handler = new ExportPeopleQueryHandler(
            new EfRepository<Person>(context),
            new EfRepository<Company>(context),
            authContext);

        var bytes = await handler.Handle(new ExportPeopleQuery(), CancellationToken.None);
        var csv = Encoding.UTF8.GetString(bytes);

        Assert.Contains("John", csv);
        Assert.Contains("john@acme.com", csv);
        Assert.Contains("Acme Inc", csv);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWorkspaceMissing()
    {
        await using var context = CreateInMemoryContext();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns((Guid?)null);

        var handler = new ExportPeopleQueryHandler(
            new EfRepository<Person>(context),
            new EfRepository<Company>(context),
            authContext);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new ExportPeopleQuery(), CancellationToken.None));
    }
}
