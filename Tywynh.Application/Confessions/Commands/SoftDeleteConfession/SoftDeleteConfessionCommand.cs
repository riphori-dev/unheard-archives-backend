using MediatR;

namespace Tywynh.Application.Confessions.Commands.SoftDeleteConfession;

public record SoftDeleteConfessionCommand(Guid Id) : IRequest<bool>;
