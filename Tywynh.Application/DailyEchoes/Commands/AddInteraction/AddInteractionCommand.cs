using MediatR;

namespace Tywynh.Application.DailyEchoes.Commands.AddInteraction
{
    public record AddInteractionCommand(
        DateTime EchoDate,
        Guid? UserId,
        string? AnonFingerprint,
        bool RitualCompleted = false,
        bool Echoed = false
    ) : IRequest<bool>;
}
