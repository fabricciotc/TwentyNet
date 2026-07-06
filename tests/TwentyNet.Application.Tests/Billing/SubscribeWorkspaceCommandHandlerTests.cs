using NSubstitute;
using TwentyNet.Application.Billing.SubscribeWorkspace;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Application.Tests.Billing;

public sealed class SubscribeWorkspaceCommandHandlerTests : TestBase
{
    [Fact]
    public async Task Handle_ShouldCreateSubscription()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        var plan = new SubscriptionPlan
        {
            Name = "Pro",
            Price = 29.99m,
            Currency = "USD",
            Interval = BillingInterval.Monthly,
            IsActive = true
        };
        context.SubscriptionPlans.Add(plan);
        await context.SaveChangesAsync();

        var authContext = Substitute.For<IAuthContext>();
        authContext.WorkspaceId.Returns(workspaceId);

        var billingService = Substitute.For<IBillingService>();
        billingService.CreateSubscriptionAsync(workspaceId, plan.Id, Arg.Any<CancellationToken>())
            .Returns(new SubscriptionResult($"sub_{plan.Id:N}", SubscriptionStatus.Active, DateTime.UtcNow.AddMonths(1)));

        var handler = new SubscribeWorkspaceCommandHandler(
            new EfRepository<WorkspaceSubscription>(context),
            new EfRepository<SubscriptionPlan>(context),
            billingService,
            MapperTestHelper.CreateMapper(),
            authContext);

        var result = await handler.Handle(new SubscribeWorkspaceCommand(plan.Id), CancellationToken.None);

        Assert.Equal("Pro", result.PlanName);
        Assert.Equal(SubscriptionStatus.Active, result.Status);
        Assert.Single(context.WorkspaceSubscriptions);
    }
}
