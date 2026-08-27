using MediatR;
using Tywynh.Application.Confessions.DTOs;
using Tywynh.Application.Common;

namespace Tywynh.Application.Confessions.Queries.GetConfessions
{
    public record GetConfessionsQuery(
        string? Category,
        string Sort,
        int Page,
        int PageSize
    ) : IRequest<PagedResult<ConfessionDto>>;
}
