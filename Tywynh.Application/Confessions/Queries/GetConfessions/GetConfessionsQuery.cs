using MediatR;
using Tywynh.Application.Confessions.DTOs;

namespace Tywynh.Application.Confessions.Queries.GetConfessions
{
    public record GetConfessionsQuery : IRequest<IEnumerable<ConfessionDto>>;
}
