using System.Data.Common;
using Microsoft.Extensions.Configuration;
using Npgsql;

var config = new ConfigurationBuilder()
    .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "Tywynh.API"))
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .Build();

var connString = config.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connString))
{
    Console.WriteLine("DefaultConnection not found in Tywynh.API/appsettings.json");
    return;
}

using var conn = new NpgsqlConnection(connString);
await conn.OpenAsync();

var cmds = new[] {
    "DROP TABLE IF EXISTS daily_echo_interactions CASCADE;",
    "DROP TABLE IF EXISTS daily_echoes CASCADE;",
    "DROP TABLE IF EXISTS resonances CASCADE;",
    "DROP TABLE IF EXISTS confessions CASCADE;",
    "DROP TABLE IF EXISTS \"__EFMigrationsHistory\" CASCADE;"
};

foreach (var sql in cmds)
{
    using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    Console.WriteLine(sql);
    await cmd.ExecuteNonQueryAsync();
}

Console.WriteLine("Dropped tables (if existed).");
