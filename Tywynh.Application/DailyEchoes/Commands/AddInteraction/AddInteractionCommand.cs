using MediatR;

namespace Tywynh.Application.DailyEchoes.Commands.AddInteraction
{
    public record AddInteractionCommand(
        DateTime EchoDate,
        string? VisitorTokenHash,
        bool RitualCompleted = false,
        bool Echoed = false
    ) : IRequest<Tywynh.Application.DailyEchoes.DTOs.EchoInteractResultDto>;
}
