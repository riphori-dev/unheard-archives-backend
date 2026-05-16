using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Domain.Entities;

namespace Tywynh.Domain.Repositories
{
    public interface IDailyEchoInteractionRepository : IBaseRepo<DailyEchoInteraction>
    {
        Task<bool> UserHasInteractedAsync(DateTime echoDate, string? anonFingerprint, CancellationToken ct = default);
        Task<DailyEchoInteraction?> GetByEchoDateAndUserAsync(DateTime echoDate, string? anonFingerprint, CancellationToken ct = default);
        Task<IEnumerable<DailyEchoInteraction>> GetByEchoDateAsync(DateTime echoDate, CancellationToken ct = default);
    }
}
