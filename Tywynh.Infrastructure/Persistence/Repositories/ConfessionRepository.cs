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
            IQueryable<Confession> query = _context.Confessions.AsNoTracking().Where(c => c.Approved && !c.Burned && c.ModerationStatus == "approved");

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
                    query = query.OrderByDescending(c => c.ResonanceCount).ThenBy(c => c.Id);
                    break;
                case "random":
                    query = query.OrderBy(c => Guid.NewGuid());
                    break;
                default:
                    query = query.OrderByDescending(c => c.CreatedAt).ThenBy(c => c.Id);
                    break;
            }

            var totalCount = await query.CountAsync(ct);
            var offset = ((long)page - 1) * pageSize;
            var items = offset >= totalCount ? new List<Confession>() : await query.Skip((int)offset).Take(pageSize).ToListAsync(ct);

            return (items, totalCount);
        }

        public async Task RequestExternalRemovalAsync(Guid id, CancellationToken ct = default) =>
            await _context.Publications.Where(p => p.ConfessionId == id && p.RemovalRequestedAt == null && p.RemovedAt == null)
                .ExecuteUpdateAsync(update => update.SetProperty(p => p.RemovalRequestedAt, (DateTime?)DateTime.UtcNow), ct);

        public async Task<IEnumerable<Confession>> GetModeratedAsync(string status, CancellationToken ct = default) => await _context.Confessions.AsNoTracking().Where(c => c.ModerationStatus == status).OrderBy(c => c.CreatedAt).ThenBy(c => c.Id).Take(100).ToListAsync(ct);
        public async Task<Confession?> SelectDailyAsync(string dateKey, CancellationToken ct = default)
        {
            var query = _context.Confessions.AsNoTracking().Where(c => c.Approved && !c.Burned && c.ModerationStatus == "approved");
            var count = await query.CountAsync(ct); if (count == 0) return null;
            uint hash = 0; foreach (var ch in dateKey) hash = unchecked(hash * 31 + ch);
            return await query.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id).Skip((int)(hash % (uint)count)).FirstOrDefaultAsync(ct);
        }
        public Task<Confession?> GetPublicByIdAsync(Guid id, CancellationToken ct = default) =>
            _context.Confessions.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.Approved && !c.Burned && c.ModerationStatus == "approved", ct);

        public async Task<Confession?> GetByIdAsync(Guid id, CancellationToken ct = default)
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