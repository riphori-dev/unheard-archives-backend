using MediatR;

namespace Tywynh.Application.DailyEchoes.Commands.SetDailyEcho
{
    public record SetDailyEchoCommand(
        DateTime Date,
        Guid ConfessionId
    ) : IRequest<bool>;
}
