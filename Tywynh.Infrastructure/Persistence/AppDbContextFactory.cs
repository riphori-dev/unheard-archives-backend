using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tywynh.Infrastructure.Persistence;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Migration tooling requires an explicit target; do not discover a different checkout's settings.
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Set ConnectionStrings__DefaultConnection explicitly before running EF tools.");

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connection, builder => builder.MigrationsAssembly("Tywynh.Infrastructure"))
            .Options;
        return new AppDbContext(options);
    }
}