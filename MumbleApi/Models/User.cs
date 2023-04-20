namespace MumbleApi.Models;

/// <summary>
/// User information inside Mumble. This information can only be fetched
/// when requested by an authenticated user.
/// </summary>
public class User : PublicUser
{
    /// <summary>
    /// Firstname of the user.
    /// </summary>
    public string Firstname { get; set; } = string.Empty;

    /// <summary>
    /// Lastname of the user.
    /// </summary>
    public string Lastname { get; set; } = string.Empty;

    public static new User FromEntity(Entities.User user) => new()
    {
        Id = user.Id,
        Firstname = user.Firstname,
        Lastname = user.Lastname,
        Username = user.Username,
        AvatarUrl = user.AvatarUrl,
    };
}
