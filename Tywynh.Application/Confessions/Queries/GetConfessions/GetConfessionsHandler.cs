using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Application.Confessions.DTOs;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.Confessions.Queries.GetConfessions
{
    public class GetConfessionsHandler : IRequestHandler<GetConfessionsQuery, Tywynh.Application.Common.PagedResult<ConfessionDto>>
    {
        private readonly IConfessionRepository _confessionRepository;

        public GetConfessionsHandler(IConfessionRepository confessionRepository)
        {
            _confessionRepository = confessionRepository;
        }

        public async Task<Tywynh.Application.Common.PagedResult<ConfessionDto>> Handle(GetConfessionsQuery request, CancellationToken cancellationToken)
        {
            var (items, totalCount) = await _confessionRepository.GetPagedAsync(
                request.Category,
                request.Sort,
                request.Page,
                request.PageSize,
                cancellationToken);

            var confessions = items.Select(confession => new ConfessionDto
            {
                Id = confession.Id,
                Text = confession.Text,
                Category = confession.Category,
                Intensity = confession.Intensity,
                Alias = confession.Alias,
                AuthorId = confession.AuthorId,
                Approved = confession.Approved,
                ResonanceCount = confession.ResonanceCount,
                EchoCount = confession.EchoCount,
                Burned = confession.Burned,
                CreatedAt = confession.CreatedAt,
                ApprovedAt = confession.ApprovedAt
            });

            return new Tywynh.Application.Common.PagedResult<ConfessionDto>(confessions, totalCount, request.Page, request.PageSize);
        }
    }
}
