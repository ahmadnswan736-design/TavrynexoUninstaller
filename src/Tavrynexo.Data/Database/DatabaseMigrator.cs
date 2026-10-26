using Dapper;
using Microsoft.Data.Sqlite;

namespace Tavrynexo.Data.Database;

public class DatabaseMigrator
{
    private readonly string _connectionString;
    public DatabaseMigrator(string connectionString) { _connectionString = connectionString; }

    public async Task<int> MigrateSchemaAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        int currentVersion = await connection.ExecuteScalarAsync<int>("PRAGMA user_version;");

        if (currentVersion < 1)
        {
            using var trans = connection.BeginTransaction();
            string v1Sql = """
                CREATE TABLE IF NOT EXISTS SchemaMigrations (
                    Version INTEGER PRIMARY KEY,
                    AppliedAtUtc TEXT NOT NULL,
                    Description TEXT NOT NULL
                );
                PRAGMA user_version = 1;
                """;
            await connection.ExecuteAsync(v1Sql, transaction: trans);
            trans.Commit();
            currentVersion = 1;
        }
        return currentVersion;
    }

    public async Task<bool> VerifyDatabaseIntegrityAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        string? result = await connection.ExecuteScalarAsync<string>("PRAGMA integrity_check;");
        return string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase);
    }
}
