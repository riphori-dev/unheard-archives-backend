using MediatR;

namespace Tywynh.Application.Confessions.Commands.ApproveConfession
{
    public record ApproveConfessionCommand(Guid Id) : IRequest<bool>;
}
