using System.ComponentModel.DataAnnotations;

namespace MumbleApi.Entities;

public class User
{
    [Key]
    public string Id { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Username { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Firstname { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Lastname { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public string? AvatarId => AvatarUrl?.Split('/').Last();

    public string? AvatarMediaType { get; set; }

    public ICollection<Post>? Posts { get; set; }

    public ICollection<Like>? Likes { get; set; }

    /// <summary>
    /// List of users that are following this user.
    /// </summary>
    public ICollection<Follow>? Followers { get; set; }

    /// <summary>
    /// List of users that this user is following.
    /// </summary>
    public ICollection<Follow>? Followees { get; set; }
}
