using MediatR;

namespace Tywynh.Application.Confessions.Commands.AddResonance
{
    public record AddResonanceCommand(Guid ConfessionId) : IRequest<bool>;
}
