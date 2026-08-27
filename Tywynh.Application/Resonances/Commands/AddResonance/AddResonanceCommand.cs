using MediatR;

namespace Tywynh.Application.Resonances.Commands.AddResonance
{
    public record AddResonanceCommand(
        Guid ConfessionId,
        Guid? UserId,
        string? VisitorTokenHash
    ) : IRequest<Tywynh.Application.Resonances.DTOs.ResonanceResultDto>;
}
