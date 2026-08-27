using Tywynh.Application.Confessions.DTOs;

namespace Tywynh.Application.DailyEchoes.DTOs;

public class DailyEchoResponseDto
{
    public ConfessionDto Confession { get; set; } = default!;
    public string Date { get; set; } = string.Empty;
    public int EchoCount { get; set; }
    public DateTime NextResetUtc { get; set; }
}
