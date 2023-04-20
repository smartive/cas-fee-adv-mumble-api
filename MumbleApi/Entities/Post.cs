using System.ComponentModel.DataAnnotations;

namespace MumbleApi.Entities;

public class Post
{
    [Key]
    public Ulid Id { get; set; } = Ulid.NewUlid();

    [Required(AllowEmptyStrings = false)]
    public string CreatorId { get; set; } = string.Empty;

    public User? Creator { get; set; }

    public string? Text { get; set; }

    public string? MediaUrl { get; set; }

    public string? MediaId => MediaUrl?.Split('/').Last();

    public string? MediaType { get; set; }

    public Ulid? ParentId { get; set; }

    public Post? Parent { get; set; }

    public DateTime? Deleted { get; set; }

    public ICollection<Like>? Likes { get; set; }

    public ICollection<Post>? Replies { get; set; }
}
