using Microsoft.Data.Sqlite;

namespace Tavrynexo.Data.Database;

public class SqliteDatabaseInitializer
{
    private readonly string _connectionString;

    public SqliteDatabaseInitializer(string? customDbPath = null)
    {
        string dbFolder = customDbPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TavrynexoUninstaller",
            "Data");

        Directory.CreateDirectory(dbFolder);
        string dbFile = Path.Combine(dbFolder, "tavrynexo.db");
        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbFile,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        }.ToString();
    }

    public string ConnectionString => _connectionString;

    public async Task InitializeDatabaseAsync()
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        using var pragmaCmd = connection.CreateCommand();
        pragmaCmd.CommandText = "PRAGMA journal_mode = WAL; PRAGMA synchronous = NORMAL; PRAGMA foreign_keys = ON;";
        await pragmaCmd.ExecuteNonQueryAsync();

        string schemaSql = """
            CREATE TABLE IF NOT EXISTS ApplicationsCache (
                Id TEXT PRIMARY KEY,
                DisplayName TEXT NOT NULL,
                DisplayVersion TEXT,
                Publisher TEXT,
                InstallDate TEXT,
                InstallLocation TEXT,
                UninstallString TEXT,
                QuietUninstallString TEXT,
                EstimatedSizeBytes INTEGER,
                Architecture TEXT,
                AppType INTEGER,
                RegistryKeyPath TEXT,
                ProductCodeGuid TEXT,
                LastScannedUtc TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS UninstallHistory (
                SessionId TEXT PRIMARY KEY,
                ApplicationName TEXT NOT NULL,
                Publisher TEXT,
                StartedAtUtc TEXT NOT NULL,
                CompletedAtUtc TEXT,
                Status TEXT NOT NULL,
                RestorePointSeq TEXT,
                RegistryBackupPath TEXT,
                QuarantinePath TEXT,
                PurgedLeftoversCount INTEGER DEFAULT 0,
                ErrorMessage TEXT
            );

            CREATE TABLE IF NOT EXISTS UninstallLeftoversLog (
                Id TEXT PRIMARY KEY,
                SessionId TEXT NOT NULL,
                ItemType INTEGER NOT NULL,
                Path TEXT NOT NULL,
                ValueName TEXT,
                SafetyLevel INTEGER NOT NULL,
                ConfidenceScore INTEGER NOT NULL,
                IsPurged INTEGER NOT NULL,
                PurgedAtUtc TEXT,
                FOREIGN KEY(SessionId) REFERENCES UninstallHistory(SessionId) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS InstallationSnapshots (
                SessionId TEXT PRIMARY KEY,
                ApplicationName TEXT NOT NULL,
                InstallerPath TEXT NOT NULL,
                CapturedAtUtc TEXT NOT NULL,
                SnapshotDataJson TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS WhitelistRules (
                Id TEXT PRIMARY KEY,
                Pattern TEXT NOT NULL,
                RuleType INTEGER NOT NULL,
                Reason TEXT,
                CreatedAtUtc TEXT NOT NULL
            );

            CREATE INDEX IF NOT EXISTS idx_apps_name ON ApplicationsCache(DisplayName);
            CREATE INDEX IF NOT EXISTS idx_history_date ON UninstallHistory(StartedAtUtc DESC);
            CREATE INDEX IF NOT EXISTS idx_leftovers_session ON UninstallLeftoversLog(SessionId);
            """;

        using var cmd = connection.CreateCommand();
        cmd.CommandText = schemaSql;
        await cmd.ExecuteNonQueryAsync();
    }
}
