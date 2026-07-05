using FluentValidation;

namespace TwentyNet.Application.Webhooks.UpdateWebhook;

public sealed class UpdateWebhookCommandValidator : AbstractValidator<UpdateWebhookCommand>
{
    private static readonly string[] SupportedEvents =
    {
        "company.created", "company.updated", "company.deleted",
        "person.created", "person.updated", "person.deleted"
    };

    public UpdateWebhookCommandValidator()
    {
        RuleFor(x => x.TargetUrl)
            .NotEmpty()
            .MaximumLength(2048)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
                         (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("TargetUrl must be a valid HTTP/HTTPS URL.");

        RuleFor(x => x.Secret)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.Events)
            .NotNull()
            .Must(events => events.Count > 0)
            .WithMessage("At least one event must be subscribed.");

        RuleForEach(x => x.Events)
            .Must(ev => SupportedEvents.Contains(ev))
            .WithMessage($"Event must be one of: {string.Join(", ", SupportedEvents)}.");
    }
}
