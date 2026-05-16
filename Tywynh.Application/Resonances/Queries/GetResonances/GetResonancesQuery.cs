using MediatR;
using Tywynh.Domain.Entities;

namespace Tywynh.Application.Resonances.Queries.GetResonances
{
    public record GetResonancesQuery(Guid? ConfessionId = null) : IRequest<IEnumerable<Resonance>>;
}
