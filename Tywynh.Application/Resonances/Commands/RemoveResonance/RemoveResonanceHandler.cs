using MediatR;
using Tywynh.Domain.Repositories;
using Tywynh.Application.Resonances.DTOs;
using Tywynh.Application.Interfaces;

namespace Tywynh.Application.Resonances.Commands.RemoveResonance;

public class RemoveResonanceHandler : IRequestHandler<RemoveResonanceCommand, ResonanceResultDto>
{
    private readonly IResonanceRepository _resonanceRepository;
    private readonly IConfessionRepository _confessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveResonanceHandler(
        IResonanceRepository resonanceRepository,
        IConfessionRepository confessionRepository,
        IUnitOfWork unitOfWork)
    {
        _resonanceRepository = resonanceRepository;
        _confessionRepository = confessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResonanceResultDto> Handle(RemoveResonanceCommand request, CancellationToken cancellationToken)
    {
        var resonance = await _resonanceRepository.GetByConfessionAndTokenAsync(request.ConfessionId, request.VisitorTokenHash, cancellationToken);
        var confession = await _confessionRepository.GetByIdAsync(request.ConfessionId, cancellationToken);
        if (confession == null)
            throw new KeyNotFoundException($"Confession with ID {request.ConfessionId} not found.");

        if (resonance != null)
        {
            await _resonanceRepository.DeleteAsync(resonance, cancellationToken);
            confession.RemoveResonance();
            await _confessionRepository.UpdateAsync(confession, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return new ResonanceResultDto(confession.ResonanceCount, false);
        }

        return new ResonanceResultDto(confession.ResonanceCount, false);
    }
}
