using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Application.Confessions.DTOs;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.Confessions.Queries.GetConfessionById
{
    public class GetConfessionByIdHandler : IRequestHandler<GetConfessionByIdQuery, ConfessionDto>
    {
        private readonly IConfessionRepository _confessionRepository;

        public GetConfessionByIdHandler(IConfessionRepository confessionRepository)
        {
            _confessionRepository = confessionRepository;
        }

        public async Task<ConfessionDto> Handle(GetConfessionByIdQuery request, CancellationToken cancellationToken)
        {
            var confession = await _confessionRepository.GetPublicByIdAsync(request.Id, cancellationToken);
            
            if (confession == null)
            {
                throw new KeyNotFoundException($"Confession with ID {request.Id} not found.");
            }

            return new ConfessionDto
            {
                Id = confession.Id,
                Text = confession.Text,
                Category = confession.Category,
                Intensity = confession.Intensity,
                Alias = confession.Alias,
                ResonanceCount = confession.ResonanceCount,
                EchoCount = confession.EchoCount,
                CreatedAt = confession.CreatedAt,
            };
        }
    }
}
