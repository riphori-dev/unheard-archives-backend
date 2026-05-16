using Tywynh.Domain.Repositories;
using Tywynh.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tywynh.Infrastructure.Persistence.Repositories
{
    public class DailyEchoInteractionRepository : IDailyEchoInteractionRepository
    {
        private readonly AppDbContext _context;

        public DailyEchoInteractionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DailyEchoInteraction> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<DailyEchoInteraction>()
                .FirstOrDefaultAsync(d => d.Id == id, ct);
        }

        public async Task AddAsync(DailyEchoInteraction interaction, CancellationToken ct = default)
        {
            await _context.Set<DailyEchoInteraction>().AddAsync(interaction, ct);
        }

        public async Task<IEnumerable<DailyEchoInteraction>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<DailyEchoInteraction>()
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task UpdateAsync(DailyEchoInteraction interaction, CancellationToken ct = default)
        {
            _context.Set<DailyEchoInteraction>().Update(interaction);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(DailyEchoInteraction interaction, CancellationToken ct = default)
        {
            _context.Set<DailyEchoInteraction>().Remove(interaction);
            await Task.CompletedTask;
        }

        public async Task<bool> UserHasInteractedAsync(DateTime echoDate, string? anonFingerprint, CancellationToken ct = default)
        {
            return await _context.Set<DailyEchoInteraction>()
                .AnyAsync(d => d.EchoDate == echoDate && 
                           d.AnonFingerprint == anonFingerprint, ct);
        }

        public async Task<DailyEchoInteraction?> GetByEchoDateAndUserAsync(DateTime echoDate, string? anonFingerprint, CancellationToken ct = default)
        {
            return await _context.Set<DailyEchoInteraction>()
                .FirstOrDefaultAsync(d => d.EchoDate == echoDate && 
                           d.AnonFingerprint == anonFingerprint, ct);
        }

        public async Task<IEnumerable<DailyEchoInteraction>> GetByEchoDateAsync(DateTime echoDate, CancellationToken ct = default)
        {
            return await _context.Set<DailyEchoInteraction>()
                .Where(d => d.EchoDate == echoDate)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(ct);
        }
    }
}
