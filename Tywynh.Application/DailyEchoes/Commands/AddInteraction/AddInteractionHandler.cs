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
    public class AddInteractionHandler : IRequestHandler<AddInteractionCommand, bool>
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

        public async Task<bool> Handle(AddInteractionCommand request, CancellationToken cancellationToken)
        {
            // Ensure daily echo exists for the date
            var dailyEcho = await _dailyEchoRepository.GetByIdAsync(request.EchoDate, cancellationToken);
            if (dailyEcho == null)
            {
                throw new KeyNotFoundException($"No daily echo found for date: {request.EchoDate:yyyy-MM-dd}");
            }

            // Check if user has already interacted with this daily echo
            var existingInteraction = await _dailyEchoInteractionRepository
                .GetByEchoDateAndUserAsync(request.EchoDate, request.AnonFingerprint, cancellationToken);

            if (existingInteraction == null)
            {
                // Create new interaction
                var interaction = DailyEchoInteraction.Create(
                    request.EchoDate,
                    request.UserId,
                    request.AnonFingerprint,
                    request.RitualCompleted,
                    request.Echoed);

                await _dailyEchoInteractionRepository.AddAsync(interaction, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
            else
            {
                // Update existing interaction
                if (request.RitualCompleted && !existingInteraction.RitualCompleted)
                {
                    existingInteraction.MarkRitualCompleted();
                }
                if (request.Echoed && !existingInteraction.Echoed)
                {
                    existingInteraction.MarkEchoed();
                }
                
                await _dailyEchoInteractionRepository.UpdateAsync(existingInteraction, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return true;
            }
        }
    }
}
