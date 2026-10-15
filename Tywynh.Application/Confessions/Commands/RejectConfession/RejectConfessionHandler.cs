using MediatR;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.Confessions.Commands.RejectConfession;

public class RejectConfessionHandler : IRequestHandler<RejectConfessionCommand, bool>
{
    private readonly IConfessionRepository _confessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectConfessionHandler(IConfessionRepository confessionRepository, IUnitOfWork unitOfWork)
    {
        _confessionRepository = confessionRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<bool> Handle(RejectConfessionCommand request, CancellationToken cancellationToken) =>
        _unitOfWork.ExecuteSerializedAsync("confession:" + request.Id, () => HandleCore(request, cancellationToken), cancellationToken);

    private async Task<bool> HandleCore(RejectConfessionCommand request, CancellationToken cancellationToken)
    {
        var confession = await _confessionRepository.GetByIdAsync(request.Id, cancellationToken);
        if (confession == null) throw new KeyNotFoundException($"Confession with ID {request.Id} not found.");

        confession.Reject(request.Reason);
        await _confessionRepository.UpdateAsync(confession, cancellationToken);
        await _confessionRepository.RequestExternalRemovalAsync(request.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
