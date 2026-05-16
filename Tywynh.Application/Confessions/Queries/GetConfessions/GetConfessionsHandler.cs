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
    public class GetConfessionsHandler : IRequestHandler<GetConfessionsQuery, IEnumerable<ConfessionDto>>
    {
        private readonly IConfessionRepository _confessionRepository;

        public GetConfessionsHandler(IConfessionRepository confessionRepository)
        {
            _confessionRepository = confessionRepository;
        }

        public async Task<IEnumerable<ConfessionDto>> Handle(GetConfessionsQuery request, CancellationToken cancellationToken)
        {
            var confessions = await _confessionRepository.GetAllAsync(cancellationToken);
            
            return confessions.Select(confession => new ConfessionDto
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
        }
    }
}
