using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Domain.ValueObjects;

namespace Tywynh.Domain.Services
{
    public interface IAliasGenerator
    {
        ConfessionAlias Generate(Guid? seed = null);
    }
}
