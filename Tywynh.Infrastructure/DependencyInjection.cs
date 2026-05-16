using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Repositories;
using Tywynh.Domain.Services;
using Tywynh.Infrastructure.Persistence;
using Tywynh.Infrastructure.Persistence.Repositories;

namespace Tywynh.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("Default");

            services.AddDbContext<AppDbContext>(
            options =>
            options.UseNpgsql(connectionString));


            // IUnitOfWork resolves to the same AppDbContext instance in the scope
            services.AddScoped<IUnitOfWork>(sp =>
                sp.GetRequiredService<AppDbContext>());

            // Repositories
            services.AddScoped<IConfessionRepository, ConfessionRepository>();
            services.AddScoped<IResonanceRepository, ResonanceRepository>();
            services.AddScoped<IDailyEchoRepository, DailyEchoRepository>();
            services.AddScoped<IDailyEchoInteractionRepository, DailyEchoInteractionRepository>();

            // Domain Services
            services.AddScoped<IAliasGenerator, AliasGenerator>();

            return services;
        }
    }
}
