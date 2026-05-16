using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.DailyEchoes.Commands.SetDailyEcho
{
    public class SetDailyEchoHandler : IRequestHandler<SetDailyEchoCommand, bool>
    {
        private readonly IDailyEchoRepository _dailyEchoRepository;
        private readonly IConfessionRepository _confessionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SetDailyEchoHandler(
            IDailyEchoRepository dailyEchoRepository,
            IConfessionRepository confessionRepository,
            IUnitOfWork unitOfWork)
        {
            _dailyEchoRepository = dailyEchoRepository;
            _confessionRepository = confessionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(SetDailyEchoCommand request, CancellationToken cancellationToken)
        {
            // Verify the confession exists and is approved
            var confession = await _confessionRepository.GetByIdAsync(request.ConfessionId, cancellationToken);
            
            if (confession == null)
            {
                throw new KeyNotFoundException($"Confession with ID {request.ConfessionId} not found.");
            }

            if (!confession.Approved)
            {
                throw new InvalidOperationException("Only approved confessions can be set as daily echo.");
            }

            // Check if a daily echo already exists for this date
            var existingEcho = await _dailyEchoRepository.GetByIdAsync(request.Date, cancellationToken);
            
            if (existingEcho != null)
            {
                // Update existing echo
                existingEcho = DailyEcho.Create(request.Date, request.ConfessionId);
                await _dailyEchoRepository.UpdateAsync(existingEcho, cancellationToken);
            }
            else
            {
                // Create new daily echo
                var dailyEcho = DailyEcho.Create(request.Date, request.ConfessionId);
                await _dailyEchoRepository.AddAsync(dailyEcho, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
