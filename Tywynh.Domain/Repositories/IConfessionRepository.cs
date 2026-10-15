using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Domain.Entities;

namespace Tywynh.Domain.Repositories
{
    public interface IConfessionRepository : IBaseRepo<Confession>
    {
        Task RequestExternalRemovalAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Confession>> GetModeratedAsync(string status, CancellationToken ct = default);
        Task<Confession?> SelectDailyAsync(string dateKey, CancellationToken ct = default);
        Task<Confession?> GetPublicByIdAsync(Guid id, CancellationToken ct = default);
        Task<(IEnumerable<Confession> Items, int TotalCount)> GetPagedAsync(
            string? category,
            string sort,
            int page,
            int pageSize,
            CancellationToken ct = default);
    }
}
