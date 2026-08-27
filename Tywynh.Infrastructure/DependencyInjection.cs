using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Repositories;
using Tywynh.Domain.Services;
using Tywynh.Infrastructure.Persistence;
using Tywynh.Infrastructure.Persistence.Repositories;

namespace Tywynh.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IConfessionRepository, ConfessionRepository>();
        services.AddScoped<IResonanceRepository, ResonanceRepository>();
        services.AddScoped<IDailyEchoRepository, DailyEchoRepository>();
        services.AddScoped<IDailyEchoInteractionRepository, DailyEchoInteractionRepository>();
        services.AddScoped<IAliasGenerator, AliasGenerator>();

        return services;
    }
}
