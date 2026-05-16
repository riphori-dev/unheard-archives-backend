using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Domain.Entities;

namespace Tywynh.Domain.Repositories
{
    public interface IResonanceRepository : IBaseRepo<Resonance>
    {
        Task<IEnumerable<Resonance>> GetByConfessionIdAsync(Guid confessionId, CancellationToken ct = default);
    }
}
