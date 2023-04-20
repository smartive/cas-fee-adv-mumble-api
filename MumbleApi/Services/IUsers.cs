using MumbleApi.Models;

using User = MumbleApi.Entities.User;

namespace MumbleApi.Services;

public interface IUsers
{
    public Task<(IEnumerable<User> Users, int TotalCount)> GetPaginatedUsers(PaginationParameters pagination);

    public Task<User?> GetUserById(string id);

    public Task UpdateUser(string id, string? firstname, string? lastname, string? username);

    public Task<string?> UpdateUserAvatar(string id, (Stream File, string MediaType)? avatar = null);

    /// <summary>
    /// Get a paginated list of users that follow the given user id.
    /// </summary>
    /// <param name="id">The user ID that is followed.</param>
    /// <param name="pagination">Parameters for pagination.</param>
    /// <returns>A tuple containing the current user page and the total count.</returns>
    public Task<(IEnumerable<User> Users, int TotalCount)> GetPaginatedFollowers(
        string id,
        PaginationParameters pagination);

    /// <summary>
    /// Get a paginated list of users that the given user id follows.
    /// </summary>
    /// <param name="id">The user ID that follows the users.</param>
    /// <param name="pagination">Pagination parameters.</param>
    /// <returns>A tuple containing the current user page and the total count.</returns>
    public Task<(IEnumerable<User> Users, int TotalCount)> GetPaginatedFollowees(
        string id,
        PaginationParameters pagination);

    public Task FollowUser(string userId, string followeeId);

    public Task UnfollowUser(string userId, string followeeId);
}
