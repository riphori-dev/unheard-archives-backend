using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Enums;

namespace Tywynh.Infrastructure.Persistence;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Confessions.AnyAsync())
            return;

        // Create sample confessions
        var c1 = Confession.Create("I lied to my best friend about where I was.", ConfessionCategory.Friendship, 3, null, "anonymous");
        c1.Approve();

        var c2 = Confession.Create("I got fired and haven't told anyone.", ConfessionCategory.Workplace, 4, null, "anon2");
        c2.Approve();

        var c3 = Confession.Create("I still have feelings for my ex.", ConfessionCategory.Romance, 2, null, "anon3");

        await context.Confessions.AddRangeAsync(new[] { c1, c2, c3 });

        // Add a daily echo for today for c1
        var today = DateTime.UtcNow.Date;
        var daily = Domain.Entities.DailyEcho.Create(today, c1.Id);
        await context.DailyEchoes.AddAsync(daily);

        // Add a resonance for c1
        var r = Domain.Entities.Resonance.Create(c1.Id, null, "seed_visitor_1_hash");
        await context.Resonances.AddAsync(r);

        await context.SaveChangesAsync();
    }
}
