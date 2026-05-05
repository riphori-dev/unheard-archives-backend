using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tywynh.Domain.ValueObjects
{
    public sealed class ConfessionAlias
    {
        public string Value { get; }

        private ConfessionAlias(string value)
        {
            Value = value;
        }

        public static ConfessionAlias Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Alias cannot be empty.");

            return new ConfessionAlias(value);
        }

        public override string ToString() => Value;
    }
}
