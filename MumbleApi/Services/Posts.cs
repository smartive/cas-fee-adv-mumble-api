using LinqKit;

using Microsoft.EntityFrameworkCore;

using MumbleApi.Database;
using MumbleApi.Errors;
using MumbleApi.Models;

using Post = MumbleApi.Entities.Post;

namespace MumbleApi.Services;

internal class Posts : IPosts
{
    private readonly IDbContextFactory<DataContext> _factory;
    private readonly IStorage _storage;

    public Posts(IDbContextFactory<DataContext> factory, IStorage storage)
    {
        _factory = factory;
        _storage = storage;
    }

    public async Task<(IEnumerable<Post> Posts, int TotalCount)> GetPaginatedPostsWithLikes(
        PostSearchParameters parameters)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var query = db.Posts
            .Include(p => p.Replies)
            .Include(p => p.Likes)
            .Include(p => p.Creator)
            .OrderByDescending(p => p.Id)
            .Where(p => p.ParentId == null)
            .Where(p => p.Deleted == null);

        if (parameters.NewerThan is not null)
        {
            query = query.Where(p => p.Id.CompareTo(parameters.NewerThan.Value) > 0);
        }

        if (parameters.OlderThan is not null)
        {
            query = query.Where(p => p.Id.CompareTo(parameters.OlderThan.Value) < 0);
        }

        if (parameters.Text is not null)
        {
            query = query.Where(p => p.Text != null && EF.Functions.ILike(p.Text, $"%{parameters.Text}%"));
        }

        if (parameters.Creators is not null)
        {
            query = query.Where(p => parameters.Creators.Contains(p.CreatorId));
        }

        if (parameters.Tags is not null)
        {
            var tagPredicate = PredicateBuilder.New<Post>();
            tagPredicate = parameters.Tags.Aggregate(
                tagPredicate,
                (current, tag) => current.Or(p => EF.Functions.ILike(p.Text!, $"%#{tag}%")));
            query = query.Where(tagPredicate);
        }

        if (parameters.LikedBy is not null)
        {
            query = query.Where(p => p.Likes!.Any(l => parameters.LikedBy.Contains(l.UserId)));
        }

        var count = await query.CountAsync();

        return (
            await query
                .Skip(parameters.Offset)
                .Take(Math.Clamp(parameters.Limit, 0, 1000))
                .ToListAsync(),
            count);
    }

    public async Task<Post?> GetPostById(Ulid id)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var query = db.Posts
            .Include(p => p.Replies)
            .Include(p => p.Likes)
            .Include(p => p.Creator)
            .Where(p => p.Deleted == null)
            .Where(p => p.ParentId == null)
            .Where(p => p.Id == id);

        return await query.SingleOrDefaultAsync();
    }

    public async Task<Post> CreatePost(
        string userId,
        Ulid? parentId = null,
        string? text = null,
        (Stream File, string MediaType)? media = null)
    {
        if (media is null && text is null)
        {
            throw new PostInvalidException();
        }

        await using var db = await _factory.CreateDbContextAsync();

        if (parentId is not null)
        {
            var parent = await db.Posts.SingleOrDefaultAsync(p => p.Id == parentId);

            if (parent is null || parent.Deleted is not null)
            {
                throw new PostNotFoundException();
            }

            if (parent.ParentId is not null)
            {
                // We cannot create a reply on a reply.
                throw new PostIsAReplyException();
            }
        }

        var post = new Post
        {
            Text = text,
            ParentId = parentId,
            Creator = await db.Users.SingleAsync(u => u.Id == userId),
        };

        if (media is not null)
        {
            post.MediaType = media.Value.MediaType;
            post.MediaUrl =
                await _storage.UploadFile(Guid.NewGuid().ToString(), media.Value.MediaType, media.Value.File);
        }

        await db.Posts.AddAsync(post);

        await db.SaveChangesAsync();
        return post;
    }

    public async Task<Post> ReplacePost(
        string userId,
        Ulid postId,
        string? text = null,
        (Stream File, string MediaType)? media = null)
    {
        if (media is null && text is null)
        {
            throw new PostInvalidException();
        }

        await using var db = await _factory.CreateDbContextAsync();

        var post = await db.Posts
            .Include(p => p.Replies)
            .Include(p => p.Likes)
            .Include(p => p.Creator)
            .Where(p => p.Id == postId)
            .Where(p => p.Deleted == null)
            .SingleOrDefaultAsync();

        if (post is null)
        {
            throw new PostNotFoundException();
        }

        if (post.CreatorId != userId)
        {
            throw new ForbiddenException();
        }

        post.Text = text;
        if (post.MediaId is not null)
        {
            await _storage.DeleteFileIfPossible(post.MediaId);
        }

        post.MediaType = post.MediaUrl = null;

        if (media is not null)
        {
            post.MediaType = media.Value.MediaType;
            post.MediaUrl =
                await _storage.UploadFile(Guid.NewGuid().ToString(), media.Value.MediaType, media.Value.File);
        }

        await db.SaveChangesAsync();
        return post;
    }

    public async Task<Post> UpdatePost(string userId, Ulid postId, string text)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var post = await db.Posts
            .Include(p => p.Replies)
            .Include(p => p.Likes)
            .Include(p => p.Creator)
            .Where(p => p.Id == postId)
            .Where(p => p.Deleted == null)
            .SingleOrDefaultAsync();

        if (post is null)
        {
            throw new PostNotFoundException();
        }

        if (post.CreatorId != userId)
        {
            throw new ForbiddenException();
        }

        post.Text = string.IsNullOrWhiteSpace(text) ? null : text;

        if (post.Text is null && post.MediaId is null)
        {
            throw new PostInvalidException();
        }

        await db.SaveChangesAsync();
        return post;
    }

    public async Task<Post> UpdatePostMedia(string userId, Ulid postId, (Stream File, string MediaType)? media = null)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var post = await db.Posts
            .Include(p => p.Replies)
            .Include(p => p.Likes)
            .Include(p => p.Creator)
            .Where(p => p.Id == postId)
            .Where(p => p.Deleted == null)
            .SingleOrDefaultAsync();

        if (post is null)
        {
            throw new PostNotFoundException();
        }

        if (post.CreatorId != userId)
        {
            throw new ForbiddenException();
        }

        if (post.Text is null && media is null)
        {
            throw new PostInvalidException();
        }

        if (post.MediaId is not null)
        {
            await _storage.DeleteFileIfPossible(post.MediaId);
        }

        post.MediaType = post.MediaUrl = null;
        if (media is not null)
        {
            post.MediaType = media.Value.MediaType;
            post.MediaUrl =
                await _storage.UploadFile(Guid.NewGuid().ToString(), media.Value.MediaType, media.Value.File);
        }

        await db.SaveChangesAsync();
        return post;
    }

    public async Task DeletePost(string userId, Ulid postId)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var post = await db.Posts
            .Where(p => p.Id == postId)
            .Where(p => p.Deleted == null)
            .SingleOrDefaultAsync();

        if (post is null)
        {
            throw new PostNotFoundException();
        }

        if (post.CreatorId != userId)
        {
            throw new ForbiddenException();
        }

        post.Deleted = DateTime.UtcNow;

        await db.SaveChangesAsync();
    }

    public async Task<bool> LikePost(string userId, Ulid postId)
    {
        await using var db = await _factory.CreateDbContextAsync();

        if (await db.Posts.SingleOrDefaultAsync(p => p.Id == postId && p.Deleted == null) is null)
        {
            throw new PostNotFoundException();
        }

        var rows = await db.Database.ExecuteSqlInterpolatedAsync($@"
            insert into likes(post_id, user_id)
            values ({postId.ToString()}, {userId})
            on conflict do nothing");

        return rows > 0;
    }

    public async Task<bool> UnlikePost(string userId, Ulid postId)
    {
        await using var db = await _factory.CreateDbContextAsync();

        if (await db.Posts.SingleOrDefaultAsync(p => p.Id == postId && p.Deleted == null) is null)
        {
            throw new PostNotFoundException();
        }

        var rows = await db.Database.ExecuteSqlInterpolatedAsync($@"
            delete from likes
            where post_id = {postId.ToString()} and user_id = {userId}");

        return rows > 0;
    }

    public async Task<(IEnumerable<Post> Posts, int TotalCount)> GetPaginatedReplies(
        Ulid postId,
        PaginationParameters pagination)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var parent = await db.Posts.SingleOrDefaultAsync(p => p.Id == postId);

        if (parent is null || parent.Deleted is not null)
        {
            throw new PostNotFoundException();
        }

        if (parent.ParentId is not null)
        {
            throw new PostIsAReplyException();
        }

        var query = db.Posts
            .Include(p => p.Likes)
            .Include(p => p.Creator)
            .OrderByDescending(p => p.Id)
            .Where(p => p.ParentId == postId)
            .Where(p => p.Deleted == null);

        var count = await query.CountAsync();

        return (
            await query
                .Skip(pagination.Offset)
                .Take(Math.Clamp(pagination.Limit, 0, 1000))
                .ToListAsync(),
            count);
    }
}
