using MediatR;

namespace Tywynh.Application.Resonances.Commands.AddResonance
{
    public record AddResonanceCommand(
        Guid ConfessionId,
        Guid? UserId,
        string? AnonFingerprint
    ) : IRequest<bool>;
}
