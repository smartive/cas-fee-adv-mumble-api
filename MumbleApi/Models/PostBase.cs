using Swashbuckle.AspNetCore.Annotations;

namespace MumbleApi.Models;

public record PostBase
{
    /// <summary>
    /// ID of the post, defined in the ULID format.
    /// </summary>
    /// <example>01GDMMR85BEHP8AKV8ZGGM259K</example>
    [SwaggerSchema(ReadOnly = true, Format = "ulid")]
    public Ulid Id { get; set; }

    /// <summary>
    /// Information about the creator of the post.
    /// </summary>
    [SwaggerSchema(ReadOnly = true)]
    public PublicUser Creator { get; set; } = new();

    /// <summary>
    /// Text for the post. Can be null if the post is only a media post.
    /// Can contain hashtags and mentions.
    /// </summary>
    /// <example>Hello World! #newpost.</example>
    public string? Text { get; set; }

    /// <summary>
    /// URL - if any - to the media object attached to this post.
    /// </summary>
    /// <example>https://storage.googleapis.com/cas-fee-adv-mumble-api/1094b5e0-5f30-4f0b-a342-ae12936c42ff</example>
    public string? MediaUrl { get; set; }

    /// <summary>
    /// If mediaUrl is set, this field contains the mime type of the media object.
    /// </summary>
    /// <example>image/png</example>
    public string? MediaType { get; set; }

    /// <summary>
    /// Number of total likes on this post.
    /// </summary>
    /// <example>42</example>
    [SwaggerSchema(ReadOnly = true)]
    public uint Likes { get; set; }

    /// <summary>
    /// Indicates if the current user liked this post. If the call was made unauthorized,
    /// this field is "null" or absent. Otherwise `true` indicates that the authorized user
    /// liked this post.
    /// </summary>
    /// <example>true|false|null</example>
    [SwaggerSchema(ReadOnly = true)]
    public bool? LikedBySelf { get; set; }
}
