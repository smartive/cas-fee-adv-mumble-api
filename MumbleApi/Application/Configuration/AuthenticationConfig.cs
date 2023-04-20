namespace MumbleApi.Application.Configuration;

public class AuthenticationConfig
{
    public string Issuer { get; init; } = string.Empty;

    public string JwtKey { get; init; } = string.Empty;

    public Zitadel.Credentials.Application ApplicationCredentials =>
        Zitadel.Credentials.Application.LoadFromJsonString(JwtKey);
}
