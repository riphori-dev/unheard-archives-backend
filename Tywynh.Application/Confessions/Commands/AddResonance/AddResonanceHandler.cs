using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.Confessions.Commands.AddResonance
{
    public class AddResonanceHandler : IRequestHandler<AddResonanceCommand, bool>
    {
        private readonly IConfessionRepository _confessionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddResonanceHandler(IConfessionRepository confessionRepository, IUnitOfWork unitOfWork)
        {
            _confessionRepository = confessionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(AddResonanceCommand request, CancellationToken cancellationToken)
        {
            var confession = await _confessionRepository.GetByIdAsync(request.ConfessionId, cancellationToken);
            
            if (confession == null)
            {
                throw new KeyNotFoundException($"Confession with ID {request.ConfessionId} not found.");
            }

            confession.AddResonance();
            await _confessionRepository.UpdateAsync(confession, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
