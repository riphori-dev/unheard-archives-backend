using MediatR;
using Tywynh.Application.Confessions.DTOs;
using Tywynh.Application.DailyEchoes.DTOs;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tywynh.Application.DailyEchoes.Queries.GetDailyEcho;

public class GetDailyEchoHandler : IRequestHandler<GetDailyEchoQuery, DailyEchoResponseDto>
{
    private readonly IDailyEchoRepository _dailyEchoRepository;
    private readonly IConfessionRepository _confessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetDailyEchoHandler(
        IDailyEchoRepository dailyEchoRepository,
        IConfessionRepository confessionRepository,
        IUnitOfWork unitOfWork)
    {
        _dailyEchoRepository = dailyEchoRepository;
        _confessionRepository = confessionRepository;
        _unitOfWork = unitOfWork;
    }

    public Task<DailyEchoResponseDto> Handle(GetDailyEchoQuery request, CancellationToken ct) =>
        _unitOfWork.ExecuteSerializedAsync("daily:" + (request.Date ?? DateTime.UtcNow).ToString("yyyy-MM-dd"), () => HandleCore(request, ct), ct);

    private async Task<DailyEchoResponseDto> HandleCore(GetDailyEchoQuery request, CancellationToken ct)
    {
        var date = request.Date?.Date ?? DateTime.UtcNow.Date;
        var dateKey = date.ToString("yyyy-MM-dd");

        var dailyEcho = await _dailyEchoRepository.GetByIdAsync(date, ct);

        if (dailyEcho == null)
        {
            var selected = await SelectDeterministicConfession(dateKey, ct);
            if (selected == null)
                throw new KeyNotFoundException("No approved confessions available for daily echo.");

            var newEcho = DailyEcho.Create(date, selected.Id);
            await _dailyEchoRepository.AddAsync(newEcho, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            dailyEcho = newEcho;
        }

        var confession = await _confessionRepository.GetByIdAsync(dailyEcho.ConfessionId, ct);
        if (confession == null || !confession.IsPublic)
            throw new KeyNotFoundException($"Confession for daily echo not found or not approved.");

        return new DailyEchoResponseDto
        {
            Confession = new ConfessionDto
            {
                Id = confession.Id,
                Text = confession.Text,
                Category = confession.Category,
                Intensity = confession.Intensity,
                Alias = confession.Alias,
                ResonanceCount = confession.ResonanceCount,
                EchoCount = confession.EchoCount,
                CreatedAt = confession.CreatedAt,
            },
            Date = dateKey,
            EchoCount = dailyEcho.EchoCount,
            NextResetUtc = date.AddDays(1)
        };
    }

    // Matches the frontend's exact algorithm:
    // hash(dateKey) % count, confessions ordered by created_at ascending
    private async Task<Confession?> SelectDeterministicConfession(
        string dateKey,
        CancellationToken ct)
    {
        return await _confessionRepository.SelectDailyAsync(dateKey, ct);
    }
}
