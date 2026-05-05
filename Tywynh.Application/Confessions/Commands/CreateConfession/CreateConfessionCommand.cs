using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Application.Confessions.DTOs;
using Tywynh.Domain.Enums;

namespace Tywynh.Application.Confessions.Commands.CreateConfession
{
    public record CreateConfessionCommand(
        string Text,
        ConfessionCategory Category,
        short Intensity,
        string Alias
    ) : IRequest<Guid>;
}
