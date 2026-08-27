using MediatR;
using Tywynh.Application.Confessions.DTOs;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.Confessions.Queries.GetModeratedConfessions;

public class GetModeratedConfessionsHandler : IRequestHandler<GetModeratedConfessionsQuery, IEnumerable<ModerationConfessionDto>>
{
    private readonly IConfessionRepository _confessionRepository;

    public GetModeratedConfessionsHandler(IConfessionRepository confessionRepository)
    {
        _confessionRepository = confessionRepository;
    }

    public async Task<IEnumerable<ModerationConfessionDto>> Handle(GetModeratedConfessionsQuery request, CancellationToken cancellationToken)
    {
        var all = await _confessionRepository.GetAllAsync(cancellationToken);
        var filtered = all.Where(c => string.Equals(c.ModerationStatus, request.Status, StringComparison.OrdinalIgnoreCase));
        return filtered.Select(c => new ModerationConfessionDto(c.Id, c.Text, c.Category.ToString(), c.ModerationStatus, c.RejectionReason, c.ModeratedAt, c.CreatedAt));
    }
}
