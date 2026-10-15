using MediatR;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.Confessions.Commands.SoftDeleteConfession;

public class SoftDeleteConfessionHandler : IRequestHandler<SoftDeleteConfessionCommand, bool>
{
    private readonly IConfessionRepository _confessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SoftDeleteConfessionHandler(IConfessionRepository confessionRepository, IUnitOfWork unitOfWork)
    {
        _confessionRepository = confessionRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<bool> Handle(SoftDeleteConfessionCommand request, CancellationToken cancellationToken) =>
        _unitOfWork.ExecuteSerializedAsync("confession:" + request.Id, () => HandleCore(request, cancellationToken), cancellationToken);

    private async Task<bool> HandleCore(SoftDeleteConfessionCommand request, CancellationToken cancellationToken)
    {
        var confession = await _confessionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (confession == null) throw new KeyNotFoundException($"Confession with ID {request.Id} not found.");

        confession.SoftDelete();
        await _confessionRepository.UpdateAsync(confession, cancellationToken);
        await _confessionRepository.RequestExternalRemovalAsync(request.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
