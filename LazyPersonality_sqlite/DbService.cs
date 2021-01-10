using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using LazyPersonality.Domain;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;
using LazyPersonality.Exceptions;

namespace LazyPersonality.Sqlite
{
    public class DbService : IDisposable
    {
        #region CommonPart
        private readonly SqliteConnection _sqlConnection;

        public DbService()
        {
            var path = FindDbPath(Directory.GetCurrentDirectory());
            _sqlConnection = new SqliteConnection($"Data Source={path}");
        }
        
        public void Dispose()
        {
            _sqlConnection.Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion
        #region Updating
        public Task AddMemUnit(MemoryUnit memoryUnitToAdd)
        {
            if (memoryUnitToAdd.Content == null)
                throw new ArgumentException("Контент не получен.");
            
            var request = $@"
                INSERT INTO memoryUnit(creationDate, content, link) 
                VALUES (
                    '{memoryUnitToAdd.CreationDate.ToString(CultureInfo.CurrentCulture)}', 
                    '{memoryUnitToAdd.Content}', 
                    '{memoryUnitToAdd.Link}');";

            return UpdateAsync(request);
        }
        public async Task BindUnitsAsync(
            MemoryUnit unit1, MemoryUnit unit2, string associationTag, DateTime creationDate, string explanation = "")
        {
            const string request = @"INSERT INTO association (creationDate, explanation, associationTag, apex1, apex2)
                                     VALUES ('{0}', '{1}', '{2}', {3}, {4});";
            
            await UpdateAsync(string.Format(request, 
                creationDate.ToString(CultureInfo.CurrentCulture),
                explanation,
                associationTag,
                unit1.Id,
                unit2.Id));
            await UpdateAsync(string.Format(request, 
                creationDate.ToString(CultureInfo.CurrentCulture),
                explanation,
                associationTag,
                unit2.Id,
                unit1.Id));
        }
        public async Task AddTagAsync(string tag)
        {
            await UpdateAsync($"INSERT INTO tag VALUES ('{tag}');");
        }
        
        public Task BindTagAsync(int memoryUnitId, string tag)
        {
            return UpdateAsync($"INSERT INTO tag_memoryUnit VALUES('{tag}', {memoryUnitId});");
        }
        
        public Task AddAssociationTagAsync(string associationTag)
        {
            return UpdateAsync($"INSERT INTO associationTag (name) VALUES ('{associationTag}');");
        }
        #endregion
        #region Getting
        public async Task<IEnumerable<MemoryUnit>> GetMemesAsync()
        {
            const string request = @"
                    SELECT mU.*, tmU.tag
                    FROM memoryUnit mU
                    LEFT JOIN tag_memoryUnit tmU ON mU.id = tmU.unit_id
                    ORDER BY mU.id; ";
            
            var result = await GetReferenceEntitiesAsync<MemoryUnit>(request, MapFunc);
            return result;

            static void MapFunc(SqliteDataReader reader, Dictionary<int, MemoryUnit> result)
            {
                var currentMemId = reader.GetInt32(0);
                if (!result.TryGetValue(currentMemId, out var currentMem))
                {
                    currentMem = new MemoryUnit
                    {
                        Id = currentMemId,
                        CreationDate = DateTime.Parse(reader.GetString(1)),
                        Content = reader.GetString(2),
                        Link = reader.GetString(3)
                    };
                    result.Add(currentMemId, currentMem);
                }
                if (!reader.IsDBNull(4))
                    currentMem.Tags.Add(reader.GetString(4));
            }
        }
        
        public Task<IEnumerable<Association>> GetAssociatedAsync(MemoryUnit memoryUnit)
        {
            var request = @$"SELECT *
                             FROM association a
                             INNER JOIN memoryUnit mU ON mU.id = a.apex2
                             WHERE a.apex1 = {memoryUnit.Id};";

            return GetEntitiesAsync<Association>(request, MapFunc);

            void MapFunc(SqliteDataReader reader, ICollection<Association> result)
            {
                var currentMemoryUnit = new MemoryUnit
                {
                    Id = reader.GetInt32(6),
                    CreationDate = DateTime.Parse(reader.GetString(7)),
                    Content = reader.GetString(8),
                    Link = reader.GetString(9)
                };
                var association = new Association
                {
                    Id = reader.GetInt32(0),
                    Date = DateTime.Parse(reader.GetString(1)),
                    Explanation = reader.GetString(2),
                    AssociationTag = reader.GetString(3),
                    Apex1 = memoryUnit,
                    Apex2 = currentMemoryUnit
                };
                result.Add(association);
            }
        }
        
        public Task<IEnumerable<string>> GetTagsAsync()
        {
            return GetEntitiesAsync<string>(
                "SELECT * FROM tag;", 
                (reader, result) => result.Add(reader.GetString(0)));
        }
        
        public Task<IEnumerable<string>> GetTagsAsync(int memoryUnitId)
        {
            return GetEntitiesAsync<string>(
                $"SELECT * FROM tag_memoryUnit WHERE unit_id={memoryUnitId};", 
                (reader, result) => result.Add(reader.GetString(0)));
        }

        public Task<IEnumerable<string>> GetAssociationTagsAsync()
        {
            return GetEntitiesAsync<string>(
                "SELECT * FROM associationTag;", 
                (reader, result) => result.Add(reader.GetString(0)));
        }
        
        public async Task<IEnumerable<MemoryUnit>> GetAllAsync()
        {
            const string request = @"SELECT * FROM association;";
            var memoryUnits = (await GetMemesAsync()).ToDictionary(m => m.Id);
            var _ = await GetEntitiesAsync<Association>(request, MapFunc);
            return memoryUnits.Values;

            void MapFunc(SqliteDataReader reader, ICollection<Association> result)
            {
                var id = reader.GetInt32(0);
                var date = reader.GetString(1);
                var explanation = reader.GetString(2);
                var tag = reader.GetString(3);
                var apex1Id = reader.GetInt32(4);
                var apex2Id = reader.GetInt32(5);
                var apex1 = memoryUnits[apex1Id];
                var apex2 = memoryUnits[apex2Id];
                var instance = new Association
                {
                    Id = id, Date = DateTime.Parse(date), Explanation = explanation, AssociationTag = tag,
                    Apex1 = apex1, Apex2 = apex2
                };
                
                apex1.Associations.Add(instance);
                result.Add(instance);
            }
        }

        public async Task<int> GetMemoryUnitId(MemoryUnit memoryUnit)
        {
            var request = @$"SELECT id FROM memoryUnit
                             WHERE content='{memoryUnit.Content}'
                             AND link='{memoryUnit.Link}'
                             AND creationDate='{memoryUnit.CreationDate.ToString(CultureInfo.CurrentCulture)}';";

            var result = (await GetEntitiesAsync<int>(request, MapFunc)).ToArray();
            if (result.Length != 1)
                throw new Exception("База содержит ...");
            return result[0];

            static void MapFunc(SqliteDataReader reader, ICollection<int> result)
            {
                result.Add(reader.GetInt32(0));
            }
        }
        #endregion
        #region Privates
        private static string FindDbPath(string path)
        {
            var results = Directory.GetFiles(path, "*.db");
            // ReSharper disable once TailRecursiveCall
            return results.Length == 0 
                ? FindDbPath(Directory.GetParent(path)!.FullName) 
                : results[0];
        }
        private async Task UpdateAsync(string request)
        {
            try
            {
                await _sqlConnection.OpenAsync();
                var command = new SqliteCommand(request, _sqlConnection);
                await command.ExecuteReaderAsync();
            }
            catch (Exception e)
            {
                throw new DbUpdatingException(
                    $"Во время обновления БД выброшено исключение. Запрос:\n{request}", e);
            }
            finally
            {
                await _sqlConnection.CloseAsync();
            }
        }

        private async Task<IEnumerable<T>> GetEntitiesAsync<T>(
            string request, Action<SqliteDataReader, ICollection<T>> mapFunc)
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var result = new List<T>();
            try
            {
                await _sqlConnection.OpenAsync();
                var command = new SqliteCommand(request, _sqlConnection);
                var reader = await command.ExecuteReaderAsync();
                while (reader.Read())
                    mapFunc(reader, result);
            }
            catch (Exception e)
            {
                throw new DbGettingException(
                    $"Во время получения данных из базы выброшено исключение. Запрос: {request}.", e);
            }
            finally
            {
                await _sqlConnection.CloseAsync();
            }

            return result;
        }

        private async Task<IEnumerable<T>> GetReferenceEntitiesAsync<T>(
            string request, Action<SqliteDataReader, Dictionary<int, T>> mapFunc)
        {
            // ReSharper disable once CollectionNeverUpdated.Local
            var result = new Dictionary<int, T>();
            try
            {
                await _sqlConnection.OpenAsync();
                var command = new SqliteCommand(request, _sqlConnection);
                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    mapFunc(reader, result);
            }
            catch (Exception e)
            {
                throw new DbGettingException(
                    $"Во время получения данных из базы выброшено исключение. Запрос: {request}.", e);
            }
            finally
            {
                await _sqlConnection.CloseAsync();
            }

            return result.Values;
        }
        #endregion
    }
}