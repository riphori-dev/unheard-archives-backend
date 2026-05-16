using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Domain.Entities;

namespace Tywynh.Domain.Repositories
{
    public interface IDailyEchoRepository : IBaseRepo<DailyEcho, DateTime>
    {
        Task<DailyEcho> GetTodayEchoAsync(CancellationToken ct = default);
        Task<bool> ExistsForDateAsync(DateTime date, CancellationToken ct = default);
    }
}
