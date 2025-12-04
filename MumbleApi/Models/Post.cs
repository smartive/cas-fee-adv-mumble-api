using System.ComponentModel;

using Swashbuckle.AspNetCore.Annotations;

namespace MumbleApi.Models;

/// <summary>
/// Post in Mumble. This is user generated content.
/// Posts can be deleted by the user who created them.
/// </summary>
public record Post : PostBase
{
    /// <summary>
    /// Number of total replies for this post.
    /// </summary>
    /// <example>42</example>
    [SwaggerSchema(ReadOnly = true)]
    public uint Replies { get; set; }

    public static Post FromEntity(Entities.Post post, string? userId = null) => new()
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
        Replies = Convert.ToUInt32(post.Replies?.Count ?? 0),
        MediaType = post.MediaType,
        MediaUrl = post.MediaUrl,
        Text = post.Text,
        LikedBySelf = userId is null ? null : (post.Likes?.Any(l => l.UserId == userId) ?? false),
    };

    public static Func<Entities.Post, Post> FromEntity(string? userId = null) => post => FromEntity(post, userId);
}
