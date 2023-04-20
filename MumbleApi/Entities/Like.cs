using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

namespace MumbleApi.Entities;

public class Like
{
    public Ulid? PostId { get; set; }

    public Post? Post { get; set; }

    [Required(AllowEmptyStrings = false)]
    public string UserId { get; set; } = string.Empty;

    public User? User { get; set; }
}
