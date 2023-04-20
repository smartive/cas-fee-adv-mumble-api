using Swashbuckle.AspNetCore.Annotations;

namespace MumbleApi.Models;

/// <summary>
/// Public user information. This information is publicly available to everyone.
/// It contains basic information such as the ID.
/// </summary>
public class PublicUser
{
    /// <summary>
    /// ID of the user who created the post.
    /// </summary>
    /// <example>179944860378202369</example>
    [SwaggerSchema(ReadOnly = true)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The username of the creator.
    /// </summary>
    /// <example>max_muster</example>
    [SwaggerSchema]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// If set, points to the avatar of the user.
    /// </summary>
    [SwaggerSchema(ReadOnly = true)]
    public string? AvatarUrl { get; set; }

    public static PublicUser FromEntity(Entities.User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        AvatarUrl = user.AvatarUrl,
    };
}
