using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TwentyNet.Application.ImportExport.ImportPeople;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.ImportExport;

public sealed class ImportPeopleCommandHandlerTests : TestBase
{
    private static ImportPeopleCommandHandler CreateHandler(
        AppDbContext context,
        IAuthContext? authContext = null,
        IRealTimeNotifier? notifier = null,
        IPublisher? publisher = null)
    {
        var personRepository = new EfRepository<Person>(context);
        var companyRepository = new EfRepository<Company>(context);
        var mapper = MapperTestHelper.CreateMapper();
        authContext ??= CreateAuthContext();
        notifier ??= Substitute.For<IRealTimeNotifier>();
        publisher ??= Substitute.For<IPublisher>();
        var logger = Substitute.For<ILogger<ImportPeopleCommandHandler>>();
        return new ImportPeopleCommandHandler(personRepository, companyRepository, mapper, authContext, notifier, publisher, logger);
    }

    private static IAuthContext CreateAuthContext(Guid? workspaceId = null)
    {
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId ?? Guid.NewGuid());
        return authContext;
    }

    private static Stream CreateCsvStream(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new MemoryStream(bytes);
    }

    [Fact]
    public async Task Handle_ShouldCreatePeople_AndLinkByCompanyName()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        context.Companies.Add(new Company { Name = "Acme Inc", WorkspaceId = workspaceId });
        await context.SaveChangesAsync();

        var handler = CreateHandler(context, CreateAuthContext(workspaceId));
        var csv = "FirstName,LastName,Email,Phone,CompanyName\nJohn,Doe,john@acme.com,+1234567890,Acme Inc";

        var result = await handler.Handle(new ImportPeopleCommand(CreateCsvStream(csv)), CancellationToken.None);

        Assert.Equal(1, result.Created);
        Assert.Equal(0, result.Updated);
        Assert.Empty(result.Errors);

        var person = context.People.Single();
        Assert.Equal("John", person.FirstName);
        Assert.Equal("Acme Inc", person.Company?.Name);
    }

    [Fact]
    public async Task Handle_ShouldUpdatePerson_WhenEmailExists()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        context.People.Add(new Person
        {
            FirstName = "Old",
            LastName = "Name",
            Email = new Domain.ValueObjects.Email("john@acme.com"),
            WorkspaceId = workspaceId
        });
        await context.SaveChangesAsync();

        var handler = CreateHandler(context, CreateAuthContext(workspaceId));
        var csv = "FirstName,LastName,Email,Phone,CompanyName\nJohn,Doe,john@acme.com,+1234567890,";

        var result = await handler.Handle(new ImportPeopleCommand(CreateCsvStream(csv)), CancellationToken.None);

        Assert.Equal(0, result.Created);
        Assert.Equal(1, result.Updated);
        Assert.Equal("John", context.People.Single().FirstName);
    }

    [Fact]
    public async Task Handle_ShouldReportError_ForInvalidEmail()
    {
        await using var context = CreateInMemoryContext();
        var handler = CreateHandler(context);
        var csv = "FirstName,LastName,Email\nJohn,Doe,not-an-email";

        var result = await handler.Handle(new ImportPeopleCommand(CreateCsvStream(csv)), CancellationToken.None);

        Assert.Equal(0, result.Created);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Handle_ShouldReportError_ForMissingNames()
    {
        await using var context = CreateInMemoryContext();
        var handler = CreateHandler(context);
        var csv = "FirstName,LastName,Email\n,Doe,john@acme.com";

        var result = await handler.Handle(new ImportPeopleCommand(CreateCsvStream(csv)), CancellationToken.None);

        Assert.Equal(0, result.Created);
        Assert.Single(result.Errors);
    }
}
