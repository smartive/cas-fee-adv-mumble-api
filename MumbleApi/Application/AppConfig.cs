using MumbleApi.Application.Configuration;

namespace MumbleApi.Application;

public class AppConfig
{
    public DatabaseConfig Database { get; init; } = new();

    public AuthenticationConfig Authentication { get; init; } = new();

    public SwaggerConfig Swagger { get; init; } = new();

    public StorageConfig Storage { get; init; } = new();
}
