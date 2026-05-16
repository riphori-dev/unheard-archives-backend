using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.Confessions.Commands.AddEcho
{
    public class AddEchoHandler : IRequestHandler<AddEchoCommand, bool>
    {
        private readonly IConfessionRepository _confessionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddEchoHandler(IConfessionRepository confessionRepository, IUnitOfWork unitOfWork)
        {
            _confessionRepository = confessionRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(AddEchoCommand request, CancellationToken cancellationToken)
        {
            var confession = await _confessionRepository.GetByIdAsync(request.ConfessionId, cancellationToken);
            
            if (confession == null)
            {
                throw new KeyNotFoundException($"Confession with ID {request.ConfessionId} not found.");
            }

            confession.AddEcho();
            await _confessionRepository.UpdateAsync(confession, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
