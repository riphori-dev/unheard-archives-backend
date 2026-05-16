using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Tywynh.Application.Confessions.DTOs;
using Tywynh.Application.Interfaces;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Repositories;
using Tywynh.Domain.Services;

namespace Tywynh.Application.Confessions.Commands.CreateConfession
{
    public class CreateConfessionHandler : IRequestHandler<CreateConfessionCommand, CreateConfessionDTO>
    {
        private readonly IConfessionRepository _confessionRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAliasGenerator _aliasGenerator;

        public CreateConfessionHandler(IConfessionRepository confessionRepo,
            IUnitOfWork unitOfWork,
            IAliasGenerator aliasGenerator)
        {
            _confessionRepo = confessionRepo;
            _unitOfWork = unitOfWork;
            _aliasGenerator = aliasGenerator;
        }

        public async Task<CreateConfessionDTO> Handle(CreateConfessionCommand command, CancellationToken ct)
        {
            var authorId = Guid.NewGuid();
            var alias = _aliasGenerator.Generate();
            // 1. Create Confession
            var confession = Confession.Create(command.Text,
                command.Category,
                command.Intensity,
                null,
                alias.Value
                );

            await _confessionRepo.AddAsync(confession, ct);
            await _unitOfWork.SaveChangesAsync(ct);

            return new CreateConfessionDTO()
            {
                Id = confession.Id,
                Alias = confession.Alias,
                Category = confession.Category,
                Intensity = confession.Intensity,
                Text = confession.Text,
            };
        }
    }
}
