using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.Confessions.Commands.ApproveConfession
{
    public class ApproveConfessionHandler : IRequestHandler<ApproveConfessionCommand, bool>
    {
        private readonly IConfessionRepository _confessionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ApproveConfessionHandler(IConfessionRepository confessionRepository, IUnitOfWork unitOfWork)
        {
            _confessionRepository = confessionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ApproveConfessionCommand request, CancellationToken cancellationToken)
        {
            var confession = await _confessionRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (confession == null)
            {
                throw new KeyNotFoundException($"Confession with ID {request.Id} not found.");
            }

            confession.Approve();
            await _confessionRepository.UpdateAsync(confession, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
