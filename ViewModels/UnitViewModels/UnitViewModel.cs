using System;
using System.Collections.Generic;
using LazyPersonality.Domain;

namespace ViewModels.UnitViewModels
{
    public class UnitViewModel : IUnitViewModel
    {
        public string Content { get; }
        public string Link { get; }
        public int Id { get; }
        public DateTime CreationDate { get; }
        public IEnumerable<string> Tags { get; }

        public UnitViewModel(MemoryUnit memoryUnit)
        {
            Content = memoryUnit.Content;
            Id = memoryUnit.Id;
            Link = memoryUnit.Link;
            CreationDate = memoryUnit.CreationDate;
            Tags = memoryUnit.Tags;
        }

        public MemoryUnit ExtractUnit()
        {
            throw new System.NotImplementedException();
        }
    }
}