using MediatR;
using Tywynh.Domain.Entities;

namespace Tywynh.Application.DailyEchoes.Queries.GetDailyEchoInteractions
{
    public record GetDailyEchoInteractionsQuery(DateTime EchoDate) : IRequest<IEnumerable<DailyEchoInteraction>>;
}
