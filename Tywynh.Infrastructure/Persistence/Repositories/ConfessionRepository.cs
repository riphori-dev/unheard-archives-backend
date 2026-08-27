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

        public async Task<(IEnumerable<Confession> Items, int TotalCount)> GetPagedAsync(
            string? category,
            string sort,
            int page,
            int pageSize,
            CancellationToken ct = default)
        {
            IQueryable<Confession> query = _context.Confessions.Where(c => c.Approved && !c.Burned);

            if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "all", StringComparison.OrdinalIgnoreCase))
            {
                if (Enum.TryParse<Tywynh.Domain.Enums.ConfessionCategory>(category, true, out var cat))
                {
                    query = query.Where(c => c.Category == cat);
                }
            }

            switch (sort)
            {
                case "resonated":
                    query = query.OrderByDescending(c => c.ResonanceCount);
                    break;
                case "random":
                    query = query.OrderBy(c => Guid.NewGuid());
                    break;
                default:
                    query = query.OrderByDescending(c => c.CreatedAt);
                    break;
            }

            var totalCount = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return (items, totalCount);
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