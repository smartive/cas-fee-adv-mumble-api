using System.ComponentModel.DataAnnotations;

namespace MumbleApi.Entities;

public class Follow
{
    [Required(AllowEmptyStrings = false)]
    public string FollowerId { get; set; } = string.Empty;

    /// <summary>
    /// The user that follows the followee.
    /// </summary>
    public User? Follower { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string FolloweeId { get; set; } = string.Empty;

    /// <summary>
    /// The user being followed by the follower.
    /// </summary>
    public User? Followee { get; set; }
}
