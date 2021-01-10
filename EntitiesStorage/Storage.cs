using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LazyPersonality.Domain;
using LazyPersonality.Sqlite;

namespace LazyPersonality.EntitiesStorage
{
    public class Storage : IDisposable
    {
        private readonly DbService _dbService;
        public List<MemoryUnit> AllUnits { get; private set; }

        public Storage()
        {
            _dbService = new DbService();
        }

        public async Task InitializeAsync()
        {
            AllUnits = (await _dbService.GetAllAsync()).ToList();
        }

        public async Task<MemoryUnit> AddUnit(MemoryUnit unit)
        {
            await _dbService.AddMemUnit(unit);
            var id = await _dbService.GetMemoryUnitId(unit);
            var result = unit with {Id = id};
            AllUnits.Add(result);
            return result;
        }

        public IEnumerable<MemoryUnit> GetUnits(params string[] tags)
        {
            return AllUnits.Where(u => ContainsAll(u, tags));
        }

        private static bool ContainsAll(MemoryUnit unit, IEnumerable<string> tags)
        {
            return tags.All(tag => unit.Tags.Contains(tag));
        }

        public void Dispose()
        {
            _dbService.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}