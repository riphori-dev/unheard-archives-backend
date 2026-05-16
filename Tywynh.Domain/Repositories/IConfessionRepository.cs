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
        new Task<Confession> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task UpdateAsync(Confession confession, CancellationToken ct = default);
        Task DeleteAsync(Confession confession, CancellationToken ct = default);
    }
}
