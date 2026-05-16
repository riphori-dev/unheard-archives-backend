using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.Resonances.Commands.AddResonance
{
    public class AddResonanceHandler : IRequestHandler<AddResonanceCommand, bool>
    {
        private readonly IResonanceRepository _resonanceRepository;
        private readonly IConfessionRepository _confessionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddResonanceHandler(
            IResonanceRepository resonanceRepository,
            IConfessionRepository confessionRepository,
            IUnitOfWork unitOfWork)
        {
            _resonanceRepository = resonanceRepository;
            _confessionRepository = confessionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(AddResonanceCommand request, CancellationToken cancellationToken)
        {
            // Get the confession to increment its resonance count
            var confession = await _confessionRepository.GetByIdAsync(request.ConfessionId, cancellationToken);
            
            if (confession == null)
            {
                throw new KeyNotFoundException($"Confession with ID {request.ConfessionId} not found.");
            }

            // Create the resonance record
            var resonance = Resonance.Create(
                request.ConfessionId,
                request.UserId,
                request.AnonFingerprint
            );

            // Increment the confession's resonance count
            confession.AddResonance();

            // Save both the resonance record and the updated confession
            await _resonanceRepository.AddAsync(resonance, cancellationToken);
            await _confessionRepository.UpdateAsync(confession, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
