using MediatR;
using Tywynh.Application.DailyEchoes.DTOs;

namespace Tywynh.Application.DailyEchoes.Queries.GetDailyEcho;

public record GetDailyEchoQuery(DateTime? Date = null) : IRequest<DailyEchoResponseDto>;
