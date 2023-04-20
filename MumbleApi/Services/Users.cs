using Microsoft.EntityFrameworkCore;

using MumbleApi.Database;
using MumbleApi.Errors;
using MumbleApi.Models;

using Npgsql;

using User = MumbleApi.Entities.User;

namespace MumbleApi.Services;

internal class Users : IUsers
{
    private readonly IDbContextFactory<DataContext> _factory;
    private readonly IStorage _storage;

    public Users(IDbContextFactory<DataContext> factory, IStorage storage)
    {
        _factory = factory;
        _storage = storage;
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetPaginatedUsers(PaginationParameters pagination)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var query = db.Users
            .OrderBy(u => u.Id);

        var count = await query.CountAsync();

        return (await query
                .Skip(pagination.Offset)
                .Take(Math.Clamp(pagination.Limit, 0, 1000))
                .ToListAsync(),
            count);
    }

    public async Task<User?> GetUserById(string id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Users.SingleOrDefaultAsync(u => u.Id == id);
    }

    public async Task UpdateUser(string id, string? firstname, string? lastname, string? username)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.SingleOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            throw new UserNotFoundException();
        }

        user.Firstname = firstname ?? user.Firstname;
        user.Lastname = lastname ?? user.Lastname;
        user.Username = username ?? user.Username;

        await db.SaveChangesAsync();
    }

    public async Task<string?> UpdateUserAvatar(string id, (Stream File, string MediaType)? avatar = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var user = await db.Users.SingleAsync(u => u.Id == id);

        if (user.AvatarId is not null)
        {
            await _storage.DeleteFileIfPossible(user.AvatarId);
        }

        user.AvatarUrl = user.AvatarMediaType = null;
        if (avatar is not null)
        {
            user.AvatarMediaType = avatar.Value.MediaType;
            user.AvatarUrl =
                await _storage.UploadFile(Guid.NewGuid().ToString(), avatar.Value.MediaType, avatar.Value.File);
        }

        await db.SaveChangesAsync();
        return user.AvatarUrl;
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetPaginatedFollowers(
        string id,
        PaginationParameters pagination)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var query = db.Follows
            .Where(f => f.FolloweeId == id)
            .Select(f => f.Follower!);

        var count = await query.CountAsync();

        return (await query
                .Skip(pagination.Offset)
                .Take(Math.Clamp(pagination.Limit, 0, 1000))
                .ToListAsync(),
            count);
    }

    public async Task<(IEnumerable<User> Users, int TotalCount)> GetPaginatedFollowees(
        string id,
        PaginationParameters pagination)
    {
        await using var db = await _factory.CreateDbContextAsync();

        var query = db.Follows
            .Where(f => f.FollowerId == id)
            .Select(f => f.Followee!);

        var count = await query.CountAsync();

        return (await query
                .Skip(pagination.Offset)
                .Take(Math.Clamp(pagination.Limit, 0, 1000))
                .ToListAsync(),
            count);
    }

    public async Task FollowUser(string userId, string followeeId)
    {
        await using var db = await _factory.CreateDbContextAsync();
        try
        {
            await db.Database.ExecuteSqlInterpolatedAsync($@"
                insert into follows(follower_id, followee_id)
                values ({userId}, {followeeId})
                on conflict do nothing");
        }
        catch (PostgresException e) when (e is { SqlState: "23503" })
        {
            throw new UserNotFoundException();
        }
    }

    public async Task UnfollowUser(string userId, string followeeId)
    {
        await using var db = await _factory.CreateDbContextAsync();

        if (!await db.Users.AnyAsync(u => u.Id == followeeId))
        {
            throw new UserNotFoundException();
        }

        await db.Database.ExecuteSqlInterpolatedAsync($@"
            delete from follows
            where follower_id = {userId} and followee_id = {followeeId}");
    }
}
