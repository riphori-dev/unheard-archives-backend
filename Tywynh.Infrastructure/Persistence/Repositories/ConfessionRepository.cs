using Tywynh.Domain.Repositories;
using Tywynh.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tywynh.Infrastructure.Persistence.Repositories
{
    public class ConfessionRepository : IConfessionRepository
    {
        private readonly AppDbContext _context;

        public ConfessionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Confession> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<Confession>()
                .FirstOrDefaultAsync(c => c.Id == id, ct);
        }

        public async Task AddAsync(Confession confession, CancellationToken ct = default)
        {
            await _context.Set<Confession>().AddAsync(confession, ct);
        }

        public async Task<IEnumerable<Confession>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<Confession>()
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task UpdateAsync(Confession confession, CancellationToken ct = default)
        {
            _context.Set<Confession>().Update(confession);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(Confession confession, CancellationToken ct = default)
        {
            _context.Set<Confession>().Remove(confession);
            await Task.CompletedTask;
        }
    }
}