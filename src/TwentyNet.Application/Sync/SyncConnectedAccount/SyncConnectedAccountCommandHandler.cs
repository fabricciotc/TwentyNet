using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Sync.SyncConnectedAccount;

public sealed class SyncConnectedAccountCommandHandler : IRequestHandler<SyncConnectedAccountCommand, SyncResult>
{
    private readonly IRepository<ConnectedAccount> _accountRepository;
    private readonly IRepository<EmailMessage> _messageRepository;
    private readonly IRepository<CalendarEvent> _eventRepository;
    private readonly IEmailCalendarSyncProvider _syncProvider;

    public SyncConnectedAccountCommandHandler(
        IRepository<ConnectedAccount> accountRepository,
        IRepository<EmailMessage> messageRepository,
        IRepository<CalendarEvent> eventRepository,
        IEmailCalendarSyncProvider syncProvider)
    {
        _accountRepository = accountRepository;
        _messageRepository = messageRepository;
        _eventRepository = eventRepository;
        _syncProvider = syncProvider;
    }

    public async Task<SyncResult> Handle(SyncConnectedAccountCommand request, CancellationToken cancellationToken)
    {
        var accounts = await _accountRepository.ListAsync(
            a => a.Id == request.ConnectedAccountId && a.IsActive,
            cancellationToken);
        var account = accounts.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Connected account {request.ConnectedAccountId} not found or inactive.");

        var since = DateTime.UtcNow.AddDays(-30);

        var messages = await _syncProvider.GetMessagesAsync(account.Id, since, cancellationToken);
        var events = await _syncProvider.GetEventsAsync(account.Id, since, cancellationToken);

        var existingMessages = await _messageRepository.ListAsync(
            m => m.ConnectedAccountId == account.Id,
            cancellationToken);
        var existingEvents = await _eventRepository.ListAsync(
            e => e.ConnectedAccountId == account.Id,
            cancellationToken);

        var messageSet = new HashSet<string>(existingMessages.Select(m => m.ExternalId));
        var eventSet = new HashSet<string>(existingEvents.Select(e => e.ExternalId));

        foreach (var input in messages.Where(m => !messageSet.Contains(m.ExternalId)))
        {
            await _messageRepository.AddAsync(new EmailMessage
            {
                ConnectedAccountId = account.Id,
                ExternalId = input.ExternalId,
                ThreadId = input.ThreadId,
                Subject = input.Subject,
                Body = input.Body,
                FromAddress = input.FromAddress,
                ToAddresses = input.ToAddresses,
                ReceivedAt = input.ReceivedAt,
                IsRead = input.IsRead
            }, cancellationToken);
        }

        foreach (var input in events.Where(e => !eventSet.Contains(e.ExternalId)))
        {
            await _eventRepository.AddAsync(new CalendarEvent
            {
                ConnectedAccountId = account.Id,
                ExternalId = input.ExternalId,
                Title = input.Title,
                Description = input.Description,
                Location = input.Location,
                StartAt = input.StartAt,
                EndAt = input.EndAt,
                IsAllDay = input.IsAllDay,
                Attendees = input.Attendees
            }, cancellationToken);
        }

        await _messageRepository.SaveChangesAsync(cancellationToken);

        return new SyncResult(messages.Count, events.Count);
    }
}
