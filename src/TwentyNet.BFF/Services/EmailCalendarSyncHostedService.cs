using MediatR;
using TwentyNet.Application.Sync.SyncConnectedAccount;
using TwentyNet.Persistence;

namespace TwentyNet.BFF.Services;

public sealed class EmailCalendarSyncHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailCalendarSyncHostedService> _logger;

    public EmailCalendarSyncHostedService(
        IServiceProvider serviceProvider,
        ILogger<EmailCalendarSyncHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email/Calendar sync hosted service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SyncAllAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to run email/calendar sync.");
            }

            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task SyncAllAsync(CancellationToken cancellationToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var accounts = context.ConnectedAccounts
            .Where(a => a.IsActive)
            .Select(a => a.Id)
            .ToList();

        foreach (var accountId in accounts)
        {
            try
            {
                var result = await sender.Send(new SyncConnectedAccountCommand(accountId), cancellationToken);
                _logger.LogInformation(
                    "Synced account {AccountId}: {Emails} emails, {Events} events.",
                    accountId,
                    result.EmailsSynced,
                    result.EventsSynced);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync account {AccountId}.", accountId);
            }
        }
    }
}
