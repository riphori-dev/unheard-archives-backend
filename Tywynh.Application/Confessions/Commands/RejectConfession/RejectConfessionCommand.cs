using MediatR;

namespace Tywynh.Application.Confessions.Commands.RejectConfession;

public record RejectConfessionCommand(Guid Id, string? Reason) : IRequest<bool>;
