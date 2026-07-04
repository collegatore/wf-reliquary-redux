using Microsoft.Data.Sqlite;
using NUnit.Framework;
using WF.Reliquary.Infrastructure.Storage;

namespace WF.Reliquary.IntegrationTests;

[TestFixture]
public sealed class SqliteDatabaseInitializerTests
{
    [Test]
    public async Task InitializeAsync_CreatesDirectoryAndDatabase_WhenPathDoesNotExist()
    {
        var dir = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        var dbPath = Path.Combine(dir, "init-test.sqlite");

        var initializer = new SqliteDatabaseInitializer(dbPath);
        await initializer.InitializeAsync();

        Assert.Multiple(() =>
        {
            Assert.That(Directory.Exists(dir), Is.True);
            Assert.That(File.Exists(dbPath), Is.True);
        });

        Assert.That(TableCount(dbPath), Is.GreaterThan(0), "Database should contain tables from migrations");
    }

    [Test]
    public async Task InitializeAsync_IsIdempotent_WhenCalledTwice()
    {
        var dbPath = TempDatabasePath();

        var initializer = new SqliteDatabaseInitializer(dbPath);
        await initializer.InitializeAsync();
        await initializer.InitializeAsync();

        Assert.That(File.Exists(dbPath), Is.True);
        Assert.That(TableCount(dbPath), Is.GreaterThan(0));
    }

    [Test]
    public async Task InitializeAsync_AppliesMigrations_ProducingExpectedTables()
    {
        var dbPath = TempDatabasePath();

        var initializer = new SqliteDatabaseInitializer(dbPath);
        await initializer.InitializeAsync();

        var tables = GetTableNames(dbPath);
        Assert.Multiple(() =>
        {
            Assert.That(tables, Does.Contain("reward_items"));
            Assert.That(tables, Does.Contain("relics"));
            Assert.That(tables, Does.Contain("profiles"));
            Assert.That(tables, Does.Contain("wishlists"));
            Assert.That(tables, Does.Contain("wishlist_items"));
        });
    }

    private static string TempDatabasePath()
    {
        var dir = Path.Combine(Path.GetTempPath(), "wf-reliquary-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "init-test.sqlite");
    }

    private static int TableCount(string dbPath)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = dbPath }.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name != '__EFMigrationsHistory'";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    private static List<string> GetTableNames(string dbPath)
    {
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = dbPath }.ConnectionString);
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' AND name != '__EFMigrationsHistory'";
        var names = new List<string>();
        using var reader = command.ExecuteReader();
        while (reader.Read()) names.Add(reader.GetString(0));
        return names;
    }
}
