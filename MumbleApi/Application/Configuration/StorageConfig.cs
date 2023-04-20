namespace MumbleApi.Application.Configuration;

public class StorageConfig
{
    public string Bucket { get; init; } = string.Empty;

    public string ServiceAccountKey { get; init; } = string.Empty;
}
