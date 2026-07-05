using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TwentyNet.Domain.Common;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Persistence.Options;
using TwentyNet.Persistence.Repositories;

namespace TwentyNet.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            var connectionStrings = provider.GetRequiredService<IOptions<ConnectionStringsOptions>>().Value;
            options.UseNpgsql(connectionStrings.DefaultConnection);
        });

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

        return services;
    }
}
