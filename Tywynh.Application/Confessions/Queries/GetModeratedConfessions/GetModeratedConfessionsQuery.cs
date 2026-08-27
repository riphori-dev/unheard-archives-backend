using MediatR;
using Tywynh.Application.Confessions.DTOs;

namespace Tywynh.Application.Confessions.Queries.GetModeratedConfessions;

public record GetModeratedConfessionsQuery(string Status) : IRequest<IEnumerable<Tywynh.Application.Confessions.DTOs.ModerationConfessionDto>>;
