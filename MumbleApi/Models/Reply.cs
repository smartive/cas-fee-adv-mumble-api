using Swashbuckle.AspNetCore.Annotations;

namespace MumbleApi.Models;

/// <summary>
/// A reply to a post.
/// </summary>
public record Reply : PostBase
{
    /// <summary>
    /// The ID of the parent post.
    /// </summary>
    [SwaggerSchema(ReadOnly = true)]
    public Ulid ParentId { get; set; }

    public static Reply FromEntity(Entities.Post post, string? userId = null) => new()
    {
        Id = post.Id,
        Creator = post.Creator is not null
            ? new()
            {
                Id = post.Creator.Id,
                Username = post.Creator.Username,
                AvatarUrl = post.Creator.AvatarUrl,
                DisplayName = post.Creator.DisplayName,
            }
            : new(),
        Likes = Convert.ToUInt32(post.Likes?.Count ?? 0),
        ParentId = post.ParentId ?? Ulid.Empty,
        MediaType = post.MediaType,
        MediaUrl = post.MediaUrl,
        Text = post.Text,
        LikedBySelf = userId is null ? null : post.Likes?.Any(l => l.UserId == userId) ?? false,
    };

    public static Func<Entities.Post, Reply> FromEntity(string? userId = null) => post => FromEntity(post, userId);
}
