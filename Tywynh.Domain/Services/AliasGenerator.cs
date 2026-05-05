using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Domain.Constants;
using Tywynh.Domain.ValueObjects;

namespace Tywynh.Domain.Services
{
    public class AliasGenerator : IAliasGenerator
    {
        private static readonly Random _random = new();

        public ConfessionAlias Generate(Guid? seed = null)
        {
            var random = seed.HasValue
                ? new Random(seed.Value.GetHashCode())
                : _random;

            var adjective = AliasWords.Adjectives[random.Next(AliasWords.Adjectives.Length)];
            var noun = AliasWords.Nouns[random.Next(AliasWords.Nouns.Length)];

            return ConfessionAlias.Create($"{adjective} {noun}");
        }
    }
}
