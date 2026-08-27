using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.DailyEchoes.Commands.AddInteraction
{
    public class AddInteractionHandler : IRequestHandler<AddInteractionCommand, Tywynh.Application.DailyEchoes.DTOs.EchoInteractResultDto>
    {
        private readonly IDailyEchoInteractionRepository _dailyEchoInteractionRepository;
        private readonly IDailyEchoRepository _dailyEchoRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddInteractionHandler(
            IDailyEchoInteractionRepository dailyEchoInteractionRepository,
            IDailyEchoRepository dailyEchoRepository,
            IUnitOfWork unitOfWork)
        {
            _dailyEchoInteractionRepository = dailyEchoInteractionRepository;
            _dailyEchoRepository = dailyEchoRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Tywynh.Application.DailyEchoes.DTOs.EchoInteractResultDto> Handle(AddInteractionCommand request, CancellationToken cancellationToken)
        {
            // Ensure daily echo exists for the date
            var dailyEcho = await _dailyEchoRepository.GetByIdAsync(request.EchoDate, cancellationToken);
            if (dailyEcho == null)
            {
                throw new KeyNotFoundException($"No daily echo found for date: {request.EchoDate:yyyy-MM-dd}");
            }

            // Check if user has already interacted with this daily echo
            var existingInteraction = await _dailyEchoInteractionRepository
                .GetByEchoDateAndUserAsync(request.EchoDate, request.VisitorTokenHash, cancellationToken);
            if (existingInteraction == null)
            {
                // Create new interaction
                var interaction = DailyEchoInteraction.Create(
                    request.EchoDate,
                    request.VisitorTokenHash,
                    request.RitualCompleted,
                    request.Echoed);

                await _dailyEchoInteractionRepository.AddAsync(interaction, cancellationToken);

                if (request.Echoed)
                {
                    dailyEcho.AddEcho();
                    await _dailyEchoRepository.UpdateAsync(dailyEcho, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new Tywynh.Application.DailyEchoes.DTOs.EchoInteractResultDto(dailyEcho.EchoCount, true);
            }
            else
            {
                // Update existing interaction
                var isNew = false;

                if (request.RitualCompleted && !existingInteraction.RitualCompleted)
                {
                    existingInteraction.MarkRitualCompleted();
                }
                if (request.Echoed && !existingInteraction.Echoed)
                {
                    existingInteraction.MarkEchoed();
                    dailyEcho.AddEcho();
                    await _dailyEchoRepository.UpdateAsync(dailyEcho, cancellationToken);
                    isNew = true;
                }

                await _dailyEchoInteractionRepository.UpdateAsync(existingInteraction, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new Tywynh.Application.DailyEchoes.DTOs.EchoInteractResultDto(dailyEcho.EchoCount, isNew);
            }
        }
    }
}
