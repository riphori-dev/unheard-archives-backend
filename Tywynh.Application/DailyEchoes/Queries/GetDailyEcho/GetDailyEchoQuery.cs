using MediatR;
using Tywynh.Application.Confessions.DTOs;

namespace Tywynh.Application.DailyEchoes.Queries.GetDailyEcho
{
    public record GetDailyEchoQuery(DateTime? Date = null) : IRequest<ConfessionDto>;
}
