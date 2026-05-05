using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tywynh.Domain.Repositories
{
    public interface IBaseRepo<T>
    {
        Task<T> GetByIdAsync(int id, CancellationToken ct = default);
        System.Threading.Tasks.Task AddAsync(T addObj, CancellationToken ct = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    }
}
