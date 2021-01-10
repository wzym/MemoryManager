using System;

namespace LazyPersonality.Domain
{
    public record Association
    {
        public int Id { get; init; }
        public DateTime Date { get; init; }
        public string Explanation { get; init; }
        public string AssociationTag { get; init; }
        public MemoryUnit Apex1 { get; init; }
        public MemoryUnit Apex2 { get; init; } // переопределить хэш на эти два
    }
}