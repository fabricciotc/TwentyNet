using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TwentyNet.Application.ImportExport.ImportCompanies;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.ImportExport;

public sealed class ImportCompaniesCommandHandlerTests : TestBase
{
    private static ImportCompaniesCommandHandler CreateHandler(
        AppDbContext context,
        IAuthContext? authContext = null,
        IRealTimeNotifier? notifier = null,
        IPublisher? publisher = null)
    {
        var repository = new EfRepository<Company>(context);
        var mapper = MapperTestHelper.CreateMapper();
        authContext ??= CreateAuthContext();
        notifier ??= Substitute.For<IRealTimeNotifier>();
        publisher ??= Substitute.For<IPublisher>();
        var logger = Substitute.For<ILogger<ImportCompaniesCommandHandler>>();
        return new ImportCompaniesCommandHandler(repository, mapper, authContext, notifier, publisher, logger);
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
    public async Task Handle_ShouldCreateCompanies_WhenNew()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var handler = CreateHandler(context, CreateAuthContext(workspaceId));
        var csv = "Name,DomainName,Address\nAcme Inc,acme.com,123 Main St";

        var result = await handler.Handle(new ImportCompaniesCommand(CreateCsvStream(csv)), CancellationToken.None);

        Assert.Equal(1, result.Created);
        Assert.Equal(0, result.Updated);
        Assert.Empty(result.Errors);

        var company = context.Companies.Single();
        Assert.Equal("Acme Inc", company.Name);
        Assert.Equal(workspaceId, company.WorkspaceId);
    }

    [Fact]
    public async Task Handle_ShouldUpdateCompany_WhenNameExists()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var existing = new Company
        {
            Name = "Acme Inc",
            DomainName = "old.com",
            WorkspaceId = workspaceId
        };
        context.Companies.Add(existing);
        await context.SaveChangesAsync();

        var handler = CreateHandler(context, CreateAuthContext(workspaceId));
        var csv = "Name,DomainName,Address\nAcme Inc,acme.com,123 Main St";

        var result = await handler.Handle(new ImportCompaniesCommand(CreateCsvStream(csv)), CancellationToken.None);

        Assert.Equal(0, result.Created);
        Assert.Equal(1, result.Updated);
        Assert.Equal("acme.com", context.Companies.Single().DomainName);
    }

    [Fact]
    public async Task Handle_ShouldSkipRows_WithEmptyName()
    {
        await using var context = CreateInMemoryContext();
        var handler = CreateHandler(context);
        var csv = "Name,DomainName,Address\n,acme.com,123 Main St";

        var result = await handler.Handle(new ImportCompaniesCommand(CreateCsvStream(csv)), CancellationToken.None);

        Assert.Equal(0, result.Created);
        Assert.Single(result.Errors);
    }

    [Fact]
    public async Task Handle_ShouldThrow_WhenWorkspaceMissing()
    {
        await using var context = CreateInMemoryContext();
        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns((Guid?)null);
        var handler = CreateHandler(context, authContext);
        var csv = "Name,DomainName,Address\nAcme Inc,acme.com,123 Main St";

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new ImportCompaniesCommand(CreateCsvStream(csv)), CancellationToken.None));
    }
}
