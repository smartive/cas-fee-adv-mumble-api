using Microsoft.AspNetCore.Mvc;

namespace MumbleApi.Models;

public record PostSearchParameters : PaginationParameters
{
    /// <summary>
    /// ID of a post. If set, only return posts that are newer than the given post ID.
    /// If omitted, no newer than filter is applied.
    /// </summary>
    /// <example>01GEESHPQQ4NJKNZJN9AKWQW6G</example>
    [FromQuery(Name = "newerThan")]
    public Ulid? NewerThan { get; set; }

    /// <summary>
    /// ID of a post. If set, only return posts that are older than the given post ID.
    /// If omitted, no older than filter is applied.
    /// </summary>
    /// <example>01GEESHPQQ4NJKNZJN9AKWQW6G</example>
    [FromQuery(Name = "olderThan")]
    public Ulid? OlderThan { get; set; }

    /// <summary>
    /// If set, search for posts that contain a specific text.
    /// </summary>
    /// <example>Hello World</example>
    [FromQuery(Name = "text")]
    public string? Text { get; set; }

    /// <summary>
    /// Search for posts that contain this tag (#TEXT).
    /// </summary>
    /// <example>["newpost", "cas"]</example>
    [FromQuery(Name = "tags")]
    public IReadOnlyList<string>? Tags { get; set; }

    /// <summary>
    /// Search for posts that were created by one of the given users (ID).
    /// All IDs must be valid user IDs and are "OR"ed.
    /// </summary>
    /// <example>["179944860378202369", "179944860378202340"]</example>
    [FromQuery(Name = "creators")]
    public IReadOnlyList<string>? Creators { get; set; }

    /// <summary>
    /// Search for posts that were liked by specific user(s).
    /// Multiple user IDs are "OR"ed (if a post is liked by user A OR B).
    /// </summary>
    /// <example>["179944860378202369", "179944860378202340"]</example>
    [FromQuery(Name = "likedBy")]
    public IReadOnlyList<string>? LikedBy { get; set; }

    public override QueryString ToQueryString()
    {
        var qs = base.ToQueryString();

        if (NewerThan.HasValue)
        {
            qs = qs.Add("newerThan", NewerThan.Value.ToString());
        }

        if (OlderThan.HasValue)
        {
            qs = qs.Add("olderThan", OlderThan.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(Text))
        {
            qs = qs.Add("text", Text);
        }

        if (Tags is not null)
        {
            qs = Tags.Aggregate(qs, (current, tag) => current.Add("tags", tag));
        }

        if (Creators is not null)
        {
            qs = Creators.Aggregate(qs, (current, creator) => current.Add("creators", creator));
        }

        if (LikedBy is not null)
        {
            qs = LikedBy.Aggregate(qs, (current, liker) => current.Add("likedBy", liker));
        }

        return qs;
    }
}
