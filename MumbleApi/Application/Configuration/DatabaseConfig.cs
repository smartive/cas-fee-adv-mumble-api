using Npgsql;

namespace MumbleApi.Application.Configuration;

public class DatabaseConfig
{
    public string ConnectionString => new NpgsqlConnectionStringBuilder
    {
        Host = Host,
        Database = Database,
        SearchPath = Schema,
        Username = Username,
        Password = Password,
        SslMode = SslMode.Disable,
        Pooling = true,
        MaxPoolSize = 10,
    }.ToString();

    public string Host { get; init; } = string.Empty;

    public string Database { get; init; } = string.Empty;

    public string Schema { get; init; } = "public";

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
