using Dapper;
using Microsoft.VisualBasic;

namespace Mattodo.Api.Data;

public class DatabaseInitializer
{
    private IDbConnectionFactory _connectionFactory;
    public DatabaseInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync(
            @"CREATE TABLE IF NOT EXISTS Tasks (
            Id TEXT PRIMARY KEY,
            Title TEXT NOT NULL,
            Details TEXT NOT NULL,
            Author TEXT NOT NULL,
            Started TEXT NOT NULL,
            Completed TEXT NOT NULL,
            LastModified TEXT NOT NULL)"
            );
    }
}
