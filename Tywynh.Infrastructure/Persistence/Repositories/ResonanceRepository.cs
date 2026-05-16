using Tywynh.Domain.Repositories;
using Tywynh.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tywynh.Infrastructure.Persistence.Repositories
{
    public class ResonanceRepository : IResonanceRepository
    {
        private readonly AppDbContext _context;

        public ResonanceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Resonance> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<Resonance>()
                .FirstOrDefaultAsync(r => r.Id == id, ct);
        }

        public async Task AddAsync(Resonance resonance, CancellationToken ct = default)
        {
            await _context.Set<Resonance>().AddAsync(resonance, ct);
        }

        public async Task<IEnumerable<Resonance>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<Resonance>()
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task UpdateAsync(Resonance resonance, CancellationToken ct = default)
        {
            _context.Set<Resonance>().Update(resonance);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Resonance resonance, CancellationToken ct = default)
        {
            _context.Set<Resonance>().Remove(resonance);
            await Task.CompletedTask;
        }

        public async Task<IEnumerable<Resonance>> GetByConfessionIdAsync(Guid confessionId, CancellationToken ct = default)
        {
            return await _context.Set<Resonance>()
                .Where(r => r.ConfessionId == confessionId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
        }

    }
}
