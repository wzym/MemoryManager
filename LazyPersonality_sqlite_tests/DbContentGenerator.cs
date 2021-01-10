using System;
using System.Collections.Generic;
using System.Linq;
using LazyPersonality.Domain;

namespace LazyPersonality.Sqlite.Tests
{
    internal static class DbContentGenerator
    {
        internal static IEnumerable<MemoryUnit> GetFullyGenerated(
            this DbService dbService, int unitsNumber, int tagsNumber)
        {
            AddAssociationTags(dbService);
            AddMemoryUnits(dbService, unitsNumber);
            AddTags(dbService, tagsNumber);
            BindTags(dbService);
            SetAssociations(dbService);

            return dbService.GetAllAsync().Result;
        }
        
        private static void SetAssociations(DbService dbService)
        {
            var registeredUnits = dbService.GetMemesAsync().Result.ToArray();
            var associationTags = dbService.GetAssociationTagsAsync().Result.ToArray();

            var prevMem = registeredUnits[0];
            var tags = NextTag().GetEnumerator();
            for (var i = 1; i < registeredUnits.Length; i++)
            {
                var currMem = registeredUnits[i];
                tags.MoveNext();
                dbService.BindUnitsAsync(prevMem, currMem, tags.Current, DateTime.Now).Wait();
                prevMem = currMem;
            }
            tags.Dispose();

            IEnumerable<string> NextTag()
            {
                var i = 0;
                while (true)
                {
                    yield return associationTags[i];
                    if (++i == associationTags.Length) i = 0;
                }
                // ReSharper disable once IteratorNeverReturns
            }
        }

        private static void BindTags(DbService dbService)
        {
            var registeredUnits = dbService.GetMemesAsync().Result.ToArray();
            var tags = dbService.GetTagsAsync().Result.ToArray();
            var rnd = new Random();
            foreach (var unit in registeredUnits)
            {
                var bindingsNumber = GetBindingsNumber();
                for (var i = 0; i < bindingsNumber; i++)
                    dbService.BindTagAsync(unit.Id, tags[i]).Wait();
            }
            
            int GetBindingsNumber() => rnd.Next(0, tags.Length + 1);
        }

        private static void AddTags(DbService dbService, int numberOfTags)
        {
            for (var i = 0; i < numberOfTags; i++)
                dbService.AddTagAsync($"Тэг {i}").Wait();
        }

        private static void AddMemoryUnits(DbService dbService, int numberOfUnits)
        {
            for (var i = 0; i < numberOfUnits; i++)
                dbService.AddMemUnit(new MemoryUnit
                {
                    Content = $"Запись {i}",
                    Link = string.Empty,
                    CreationDate = DateTime.Now
                }).Wait();
        }

        private static void AddAssociationTags(DbService dbService)
        {
            dbService.AddAssociationTagAsync("неквалифицированная ассоциация").Wait();
            dbService.AddAssociationTagAsync("абстрагирование/конкретизация").Wait();
            dbService.AddAssociationTagAsync("идея/иллюстрация").Wait();
        }
    }
}