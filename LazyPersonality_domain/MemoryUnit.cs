using System;
using System.Collections.Generic;

namespace LazyPersonality.Domain
{
    public record MemoryUnit
    {
        public int Id { get; init; }
        public DateTime CreationDate { get; init; }
        public string Content { get; init; } = null!;
        public string Link { get; init; } = null!;
        public HashSet<string> Tags { get; } = new();
        public HashSet<Association> Associations { get; } = new();
    }
}