using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace LazyPersonality.Sqlite
{
    public static class DbCreator
    {
        private const string AssociationTableCreateRequest = @"
            CREATE TABLE association (
            id	INTEGER NOT NULL,
            creationDate	TEXT NOT NULL,
            explanation	TEXT,
            associationTag	TEXT NOT NULL,
            apex1	INTEGER NOT NULL,
            apex2	INTEGER NOT NULL CHECK(apex1 <> apex2),
            FOREIGN KEY(apex1) REFERENCES memoryUnit(id),
            FOREIGN KEY(apex2) REFERENCES memoryUnit(id),
            FOREIGN KEY(associationTag) REFERENCES associationTag(name),
            PRIMARY KEY(id AUTOINCREMENT));";
        
        private const string AssociationTagTableCreateRequest = @"
            CREATE TABLE associationTag (
            name TEXT NOT NULL,
            PRIMARY KEY(name)
            ) WITHOUT ROWID;";

        private const string MemoryUnitTableCreateRequest = @"
            CREATE TABLE memoryUnit (
            id	            INTEGER NOT NULL DEFAULT NULL,
            creationDate	TEXT NOT NULL,
            content	        TEXT NOT NULL,
            link	        TEXT,
            PRIMARY KEY(id AUTOINCREMENT));";

        private const string TagTableCreateRequest = @"
            CREATE TABLE tag (
            name	TEXT NOT NULL,
            PRIMARY KEY(name)
            ) WITHOUT ROWID;";

        private const string TagMemoryUnitTableCreateRequest = @"
            CREATE TABLE tag_memoryUnit (
            tag	TEXT NOT NULL,
            unit_id	INTEGER NOT NULL,
            FOREIGN KEY(unit_id) REFERENCES memoryUnit(id),
            FOREIGN KEY(tag) REFERENCES tag(name),
            PRIMARY KEY(tag,unit_id));";

        public static async Task CreateTables()
        {
            await using var connection = new SqliteConnection($"Data Source={Directory.GetCurrentDirectory()}\\dbSource1.db");
            await connection.OpenAsync();
            var commands = new Task[]
            {
                new SqliteCommand(AssociationTableCreateRequest, connection).ExecuteNonQueryAsync(),
                new SqliteCommand(AssociationTagTableCreateRequest, connection).ExecuteNonQueryAsync(),
                new SqliteCommand(MemoryUnitTableCreateRequest, connection).ExecuteNonQueryAsync(),
                new SqliteCommand(TagTableCreateRequest, connection).ExecuteNonQueryAsync(),
                new SqliteCommand(TagMemoryUnitTableCreateRequest, connection).ExecuteNonQueryAsync(),
            };

            await Task.WhenAll(commands);
        }
    }
}