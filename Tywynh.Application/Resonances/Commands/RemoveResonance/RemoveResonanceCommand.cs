using MediatR;

namespace Tywynh.Application.Resonances.Commands.RemoveResonance;

public record RemoveResonanceCommand(Guid ConfessionId, string VisitorTokenHash) : IRequest<Tywynh.Application.Resonances.DTOs.ResonanceResultDto>;
