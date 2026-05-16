using MediatR;
using Tywynh.Application.Confessions.DTOs;

namespace Tywynh.Application.Confessions.Queries.GetConfessionById
{
    public record GetConfessionByIdQuery(Guid Id) : IRequest<ConfessionDto>;
}
