using System.Text.Json;

using Lib.AspNetCore.ServerSentEvents;

using Microsoft.Extensions.Options;

using MumbleApi.Models;

namespace MumbleApi.Services;

internal class PostUpdates(IOptions<ServerSentEventsServiceOptions<ServerSentEventsService>> options)
    : ServerSentEventsService(options), IPostUpdates
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
    };

    public Task NewPost(PostBase post)
        => SendEventAsync(new ServerSentEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = "postCreated",
            Data = new List<string> { JsonSerializer.Serialize(post, Json) },
        });

    public Task PostUpdated(PostBase post)
        => SendEventAsync(new ServerSentEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = "postUpdated",
            Data = new List<string> { JsonSerializer.Serialize(post, Json) },
        });

    public Task PostDeleted(Ulid postId)
        => SendEventAsync(new ServerSentEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = "postDeleted",
            Data = new List<string> { JsonSerializer.Serialize(new { Id = postId }, Json) },
        });

    public Task PostLiked(string userId, Ulid postId)
        => SendEventAsync(new ServerSentEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = "postLiked",
            Data = new List<string> { JsonSerializer.Serialize(new { UserId = userId, PostId = postId }, Json) },
        });

    public Task PostUnliked(string userId, Ulid postId)
        => SendEventAsync(new ServerSentEvent
        {
            Id = Guid.NewGuid().ToString(),
            Type = "postUnliked",
            Data = new List<string> { JsonSerializer.Serialize(new { UserId = userId, PostId = postId }, Json) },
        });
}
