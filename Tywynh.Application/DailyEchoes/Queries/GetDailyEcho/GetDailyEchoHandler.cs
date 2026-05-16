using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Application.Confessions.DTOs;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Repositories;

namespace Tywynh.Application.DailyEchoes.Queries.GetDailyEcho
{
    public class GetDailyEchoHandler : IRequestHandler<GetDailyEchoQuery, ConfessionDto>
    {
        private readonly IDailyEchoRepository _dailyEchoRepository;
        private readonly IConfessionRepository _confessionRepository;

        public GetDailyEchoHandler(IDailyEchoRepository dailyEchoRepository, IConfessionRepository confessionRepository)
        {
            _dailyEchoRepository = dailyEchoRepository;
            _confessionRepository = confessionRepository;
        }

        public async Task<ConfessionDto> Handle(GetDailyEchoQuery request, CancellationToken cancellationToken)
        {
            var date = request.Date?.Date ?? DateTime.UtcNow.Date;
            
            var dailyEcho = await _dailyEchoRepository.GetByIdAsync(date, cancellationToken);
            
            if (dailyEcho == null)
            {
                // If no daily echo is set for today, try to auto-select one
                var selectedConfession = await SelectRandomConfession(cancellationToken);
                if (selectedConfession == null)
                {
                    throw new KeyNotFoundException("No confessions available for daily echo.");
                }

                // Create and save the daily echo
                var newDailyEcho = DailyEcho.Create(date, selectedConfession.Id);
                await _dailyEchoRepository.AddAsync(newDailyEcho, cancellationToken);
                // Note: You might want to save changes here or let the caller handle it
                
                dailyEcho = newDailyEcho;
            }

            // Get the full confession details
            var confession = await _confessionRepository.GetByIdAsync(dailyEcho.ConfessionId, cancellationToken);
            
            if (confession == null)
            {
                throw new KeyNotFoundException($"Confession with ID {dailyEcho.ConfessionId} not found.");
            }

            return new ConfessionDto
            {
                Id = confession.Id,
                Text = confession.Text,
                Category = confession.Category,
                Intensity = confession.Intensity,
                Alias = confession.Alias,
                AuthorId = confession.AuthorId,
                Approved = confession.Approved,
                ResonanceCount = confession.ResonanceCount,
                EchoCount = confession.EchoCount,
                Burned = confession.Burned,
                CreatedAt = confession.CreatedAt,
                ApprovedAt = confession.ApprovedAt
            };
        }

        private async Task<Confession?> SelectRandomConfession(CancellationToken cancellationToken)
        {
            // Get all approved confessions that haven't been recent daily echoes
            var allConfessions = await _confessionRepository.GetAllAsync(cancellationToken);
            var approvedConfessions = allConfessions.Where(c => c.Approved && !c.Burned).ToList();
            
            if (!approvedConfessions.Any())
                return null;

            // Get recent daily echoes to avoid repetition (last 30 days)
            var recentEchoes = await _dailyEchoRepository.GetAllAsync(cancellationToken);
            var recentConfessionIds = recentEchoes
                .Where(d => d.EchoDate >= DateTime.UtcNow.Date.AddDays(-30))
                .Select(d => d.ConfessionId)
                .ToHashSet();

            // Filter out recently used confessions
            var availableConfessions = approvedConfessions
                .Where(c => !recentConfessionIds.Contains(c.Id))
                .ToList();

            // If no available confessions after filtering, use all approved confessions
            if (!availableConfessions.Any())
                availableConfessions = approvedConfessions;

            // Select randomly
            var random = new Random();
            return availableConfessions[random.Next(availableConfessions.Count)];
        }
    }
}
