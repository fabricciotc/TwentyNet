using Microsoft.Extensions.Options;
using TwentyNet.BFF.Options;
using TwentyNet.Persistence.Options;

namespace TwentyNet.BFF;

public static class DependencyInjection
{
    public static IServiceCollection AddBffServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConnectionStringsOptions>(configuration.GetSection(ConnectionStringsOptions.SectionName));
        services.Configure<HttpClientOptions>(configuration.GetSection(HttpClientOptions.SectionName));

        services.AddHttpClient(HttpClientOptions.EnrichmentClientName, (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
            client.BaseAddress = new Uri(options.EnrichmentBaseAddress);
            client.Timeout = TimeSpan.FromSeconds(options.EnrichmentTimeoutSeconds);
        });

        return services;
    }
}
