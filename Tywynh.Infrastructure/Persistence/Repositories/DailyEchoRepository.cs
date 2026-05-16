using Tywynh.Domain.Repositories;
using Tywynh.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tywynh.Infrastructure.Persistence.Repositories
{
    public class DailyEchoRepository : IDailyEchoRepository
    {
        private readonly AppDbContext _context;

        public DailyEchoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DailyEcho> GetByIdAsync(DateTime date, CancellationToken ct = default)
        {
            return await _context.Set<DailyEcho>()
                .Include(d => d.Confession)
                .FirstOrDefaultAsync(d => d.EchoDate == date.Date, ct);
        }

        public async Task AddAsync(DailyEcho dailyEcho, CancellationToken ct = default)
        {
            await _context.Set<DailyEcho>().AddAsync(dailyEcho, ct);
        }

        public async Task<IEnumerable<DailyEcho>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<DailyEcho>()
                .Include(d => d.Confession)
                .OrderByDescending(d => d.EchoDate)
                .ToListAsync(ct);
        }

        public async Task UpdateAsync(DailyEcho dailyEcho, CancellationToken ct = default)
        {
            _context.Set<DailyEcho>().Update(dailyEcho);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(DailyEcho dailyEcho, CancellationToken ct = default)
        {
            _context.Set<DailyEcho>().Remove(dailyEcho);
            await Task.CompletedTask;
        }

        public async Task<DailyEcho> GetTodayEchoAsync(CancellationToken ct = default)
        {
            return await GetByIdAsync(DateTime.UtcNow, ct);
        }

        public async Task<bool> ExistsForDateAsync(DateTime date, CancellationToken ct = default)
        {
            return await _context.Set<DailyEcho>()
                .AnyAsync(d => d.EchoDate == date.Date, ct);
        }
    }
}
