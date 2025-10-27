using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace SpaceXLaunchDataService.Api.Common.Services.Infrastructure.Database;

public interface IDatabaseConnectionFactory
{
    IDbConnection CreateConnection();
    Task InitializeDatabaseAsync();
    void EnsureDatabaseExists();
}

public class SqliteDatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;
    private readonly string _databaseDirectory;
    private readonly string _databasePath;

    public SqliteDatabaseConnectionFactory(IConfiguration configuration)
    {
        // Get the solution root directory (where .sln file is)
        var solutionRoot = FindSolutionRoot();
        _databaseDirectory = Path.Combine(solutionRoot, "database");
        _databasePath = Path.Combine(_databaseDirectory, "spacex_launches.db");

        _connectionString = $"Data Source={_databasePath}";
    }

    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(_connectionString);
    }

    public async Task InitializeDatabaseAsync()
    {
        // Ensure database directory exists
        if (!Directory.Exists(_databaseDirectory))
        {
            Directory.CreateDirectory(_databaseDirectory);
        }

        // Create database and tables if they don't exist
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();

        var createTableSql = """
            CREATE TABLE IF NOT EXISTS Launches (
                Id TEXT PRIMARY KEY,
                FlightNumber INTEGER NOT NULL,
                Name TEXT NOT NULL,
                DateUtc TEXT NOT NULL,
                Success INTEGER NULL,
                Details TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE INDEX IF NOT EXISTS idx_launches_flight_number ON Launches(FlightNumber);
            CREATE INDEX IF NOT EXISTS idx_launches_date_utc ON Launches(DateUtc);
            CREATE INDEX IF NOT EXISTS idx_launches_success ON Launches(Success);
            CREATE INDEX IF NOT EXISTS idx_launches_name ON Launches(Name);
            """;

        await connection.ExecuteAsync(createTableSql);
    }

    public void EnsureDatabaseExists()
    {
        // Ensure database directory exists
        if (!Directory.Exists(_databaseDirectory))
        {
            Directory.CreateDirectory(_databaseDirectory);
        }

        // Create database and tables if they don't exist (synchronous version for startup)
        using var connection = CreateConnection();
        connection.Open();

        var createTableSql = """
            CREATE TABLE IF NOT EXISTS Launches (
                Id TEXT PRIMARY KEY,
                FlightNumber INTEGER NOT NULL,
                Name TEXT NOT NULL,
                DateUtc TEXT NOT NULL,
                Success INTEGER NULL,
                Details TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );

            CREATE INDEX IF NOT EXISTS idx_launches_flight_number ON Launches(FlightNumber);
            CREATE INDEX IF NOT EXISTS idx_launches_date_utc ON Launches(DateUtc);
            CREATE INDEX IF NOT EXISTS idx_launches_success ON Launches(Success);
            CREATE INDEX IF NOT EXISTS idx_launches_name ON Launches(Name);
            """;

        connection.Execute(createTableSql);
    }

    private static string FindSolutionRoot()
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        // Look for .sln file going up the directory tree
        while (currentDirectory != null)
        {
            if (Directory.GetFiles(currentDirectory, "*.sln").Any())
            {
                return currentDirectory;
            }

            var parent = Directory.GetParent(currentDirectory);
            currentDirectory = parent?.FullName;
        }

        // Fallback to current directory if no .sln file found
        return Directory.GetCurrentDirectory();
    }
}