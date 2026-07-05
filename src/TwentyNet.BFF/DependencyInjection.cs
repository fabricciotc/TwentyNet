using System.Text;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TwentyNet.Application.Webhooks;
using TwentyNet.BFF.Hubs;
using TwentyNet.BFF.Options;
using TwentyNet.BFF.Services;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence.Options;

namespace TwentyNet.BFF;

public static class DependencyInjection
{
    public static IServiceCollection AddBffServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ConnectionStringsOptions>(configuration.GetSection(ConnectionStringsOptions.SectionName));
        services.Configure<HttpClientOptions>(configuration.GetSection(HttpClientOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];

                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("role", WorkspaceRole.Admin.ToString()));

            options.AddPolicy("RequireMember", policy =>
                policy.RequireAuthenticatedUser()
                    .RequireClaim("role", WorkspaceRole.Admin.ToString(), WorkspaceRole.Member.ToString()));
        });
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthContext, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IRealTimeNotifier, SignalRRealTimeNotifier>();
        services.AddScoped<IWebhookDeliveryService, WebhookDeliveryService>();
        services.AddSingleton<ITokenEncryptionService, TokenEncryptionService>();
        services.AddScoped<ISecureHttpClient, SecureHttpClient>();

        services.AddDataProtection();

        services.AddHttpClient(HttpClientOptions.EnrichmentClientName, (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<HttpClientOptions>>().Value;
            client.BaseAddress = new Uri(options.EnrichmentBaseAddress);
            client.Timeout = TimeSpan.FromSeconds(options.EnrichmentTimeoutSeconds);
        });

        services.AddHttpClient(HttpClientOptions.WebhookClientName);

        AddStorage(services);

        return services;
    }

    private static void AddStorage(IServiceCollection services)
    {
        services.AddSingleton<IAmazonS3>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<StorageOptions>>().Value;

            if (!string.Equals(options.Provider, "S3", StringComparison.OrdinalIgnoreCase))
            {
                // Return a dummy client for non-S3 providers. It won't be used because
                // IStorageDriver is resolved to LocalStorageDriver below.
                return new AmazonS3Client(new BasicAWSCredentials("dummy", "dummy"), RegionEndpoint.USEast1);
            }

            var credentials = new BasicAWSCredentials(options.S3AccessKey, options.S3SecretKey);

            if (!string.IsNullOrWhiteSpace(options.S3Endpoint))
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = options.S3Endpoint,
                    ForcePathStyle = options.S3ForcePathStyle,
                    RegionEndpoint = RegionEndpoint.GetBySystemName(options.S3Region)
                };

                return new AmazonS3Client(credentials, config);
            }

            return new AmazonS3Client(credentials, RegionEndpoint.GetBySystemName(options.S3Region));
        });

        services.AddScoped<IStorageDriver>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<StorageOptions>>().Value;

            if (string.Equals(options.Provider, "S3", StringComparison.OrdinalIgnoreCase))
            {
                var client = provider.GetRequiredService<IAmazonS3>();
                return new S3StorageDriver(client, provider.GetRequiredService<IOptions<StorageOptions>>());
            }

            return new LocalStorageDriver(provider.GetRequiredService<IOptions<StorageOptions>>());
        });
    }
}
