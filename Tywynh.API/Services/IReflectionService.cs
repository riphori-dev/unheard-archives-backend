using System.Threading;
using System.Threading.Tasks;

namespace Tywynh.API.Services;

public interface IReflectionService
{
    Task<string?> GenerateReflectionAsync(string confessionText, CancellationToken cancellationToken);
}
