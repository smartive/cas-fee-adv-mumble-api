using MumbleApi.Models;

using Post = MumbleApi.Entities.Post;

namespace MumbleApi.Services;

public interface IPosts
{
    public Task<(IEnumerable<Post> Posts, int TotalCount)> GetPaginatedPostsWithLikes(PostSearchParameters parameters);

    public Task<Post?> GetPostById(Ulid id);

    public Task<Post> CreatePost(
        string userId,
        Ulid? parentId = null,
        string? text = null,
        (Stream File, string MediaType)? media = null);

    public Task<Post> ReplacePost(
        string userId,
        Ulid postId,
        string? text = null,
        (Stream File, string MediaType)? media = null);

    public Task<Post> UpdatePost(string userId, Ulid postId, string text);

    public Task<Post> UpdatePostMedia(string userId, Ulid postId, (Stream File, string MediaType)? media = null);

    public Task DeletePost(string userId, Ulid postId);

    public Task<bool> LikePost(string userId, Ulid postId);

    public Task<bool> UnlikePost(string userId, Ulid postId);

    public Task<(IEnumerable<Post> Posts, int TotalCount)> GetPaginatedReplies(
        Ulid postId,
        PaginationParameters pagination);
}
