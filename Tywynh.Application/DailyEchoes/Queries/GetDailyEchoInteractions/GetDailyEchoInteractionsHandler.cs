using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.DailyEchoes.Queries.GetDailyEchoInteractions
{
    public class GetDailyEchoInteractionsHandler : IRequestHandler<GetDailyEchoInteractionsQuery, IEnumerable<DailyEchoInteraction>>
    {
        private readonly IDailyEchoInteractionRepository _dailyEchoInteractionRepository;

        public GetDailyEchoInteractionsHandler(IDailyEchoInteractionRepository dailyEchoInteractionRepository)
        {
            _dailyEchoInteractionRepository = dailyEchoInteractionRepository;
        }

        public async Task<IEnumerable<DailyEchoInteraction>> Handle(
            GetDailyEchoInteractionsQuery request,
            CancellationToken cancellationToken)
        {
            return await _dailyEchoInteractionRepository.GetByEchoDateAsync(request.EchoDate, cancellationToken);
        }
    }
}
