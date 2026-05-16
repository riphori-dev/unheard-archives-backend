using MediatR;

namespace Tywynh.Application.Confessions.Commands.AddEcho
{
    public record AddEchoCommand(Guid ConfessionId) : IRequest<bool>;
}
