using Lib.AspNetCore.ServerSentEvents;

using MumbleApi.Models;

namespace MumbleApi.Services;

public interface IPostUpdates : IServerSentEventsService
{
    public Task NewPost(PostBase post);

    public Task PostUpdated(PostBase post);

    public Task PostDeleted(Ulid postId);

    public Task PostLiked(string userId, Ulid postId);

    public Task PostUnliked(string userId, Ulid postId);
}
