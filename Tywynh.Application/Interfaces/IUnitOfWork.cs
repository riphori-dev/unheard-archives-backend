using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tywynh.Application.Interfaces
{
    public interface IUnitOfWork
    {
        Task<T> ExecuteSerializedAsync<T>(string key, Func<Task<T>> action, CancellationToken ct = default);
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
