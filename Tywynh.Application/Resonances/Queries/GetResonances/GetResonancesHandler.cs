using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.Resonances.Queries.GetResonances
{
    public class GetResonancesHandler : IRequestHandler<GetResonancesQuery, IEnumerable<Resonance>>
    {
        private readonly IResonanceRepository _resonanceRepository;

        public GetResonancesHandler(IResonanceRepository resonanceRepository)
        {
            _resonanceRepository = resonanceRepository;
        }

        public async Task<IEnumerable<Resonance>> Handle(GetResonancesQuery request, CancellationToken cancellationToken)
        {
            if (request.ConfessionId.HasValue)
            {
                return await _resonanceRepository.GetByConfessionIdAsync(request.ConfessionId.Value, cancellationToken);
            }

            return await _resonanceRepository.GetAllAsync(cancellationToken);
        }
    }
}
