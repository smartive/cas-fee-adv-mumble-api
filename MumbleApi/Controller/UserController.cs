using Microsoft.AspNetCore.Mvc;

using MumbleApi.Application;
using MumbleApi.Authentication;
using MumbleApi.Errors;
using MumbleApi.Models;
using MumbleApi.Services;

using Swashbuckle.AspNetCore.Annotations;

namespace MumbleApi.Controller;

[ApiController]
[Route("users")]
[OptionalZitadelAuthorize]
[Produces("application/json")]
[SwaggerTag("Users in the Mumble System.")]
public class UserController(IUsers users) : ControllerBase
{
    /// <summary>
    /// Fetch a paginated list of users.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Description =
        "Fetch a paginated list of users. " +
        "If the caller is authenticated, the user list will contain more data.")]
    [SwaggerResponse(200, "Success", typeof(PaginatedResult<User>))]
    public async Task<IActionResult> Get([FromQuery] PaginationParameters pagination)
    {
        var (dbUsers, total) = await users.GetPaginatedUsers(pagination);
        var loggedIn = HttpContext.User.Identity?.IsAuthenticated == true;

        var next = total > pagination.Offset + pagination.Limit
            ? $"{Url.ActionLink()}{(pagination with { Offset = pagination.Offset + pagination.Limit }).ToQueryString()}"
            : null;
        var prev = pagination.Offset > 0
            ? $"{Url.ActionLink()}{(pagination with { Offset = Math.Max(pagination.Offset - pagination.Limit, 0) }).ToQueryString()}"
            : null;

        return loggedIn
            ? Ok(new PaginatedResult<User>
            {
                Count = Convert.ToUInt32(total),
                Data = dbUsers.Select(Models.User.FromEntity).ToList(),
                Next = next,
                Previous = prev,
            })
            : Ok(new PaginatedResult<PublicUser>
            {
                Count = Convert.ToUInt32(total),
                Data = dbUsers.Select(PublicUser.FromEntity).ToList(),
                Next = next,
                Previous = prev,
            });
    }

    /// <summary>
    /// Get a specific user by their ID.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(Description =
        "Fetch a specific user profile from the API. " +
        "If the caller is authenticated, more information than the public profile is provided.")]
    [SwaggerResponse(200, "Success", typeof(User))]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> GetById(string id)
    {
        var user = await users.GetUserById(id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(HttpContext.User.Identity?.IsAuthenticated == true
            ? Models.User.FromEntity(user)
            : PublicUser.FromEntity(user));
    }

    /// <summary>
    /// Update the user profile.
    /// </summary>
    [HttpPatch]
    [ZitadelAuthorize]
    [SwaggerOperation(Description =
        "Update the current authenticated user profile. " +
        "Returns a bad request if fields are set to an empty string. " +
        "Omitting a field does not update them. " +
        "Uploading an avatar is done via the /users/avatar endpoint.")]
    [SwaggerResponse(204, "Success - No Content")]
    [SwaggerResponse(400, "Bad Request")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserData data)
    {
        if (data.Firstname == string.Empty)
        {
            return BadRequest("Firstname is an empty string.");
        }

        if (data.Lastname == string.Empty)
        {
            return BadRequest("Lastname is an empty string.");
        }

        if (data.Username == string.Empty)
        {
            return BadRequest("Username is an empty string.");
        }

        try
        {
            await users.UpdateUser(HttpContext.UserId(), data.Firstname, data.Lastname, data.Username);
            return NoContent();
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Upload an avatar.
    /// </summary>
    [HttpPut("avatar")]
    [ZitadelAuthorize]
    [Produces("text/plain")]
    [RequestSizeLimit(512 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 512 * 1024)]
    [SwaggerOperation(Description =
        "Upload an avatar picture for the actual authenticated user. " +
        "Returns the new media url to the uploaded user avatar. " +
        "Upload limit: 0.5 MB.")]
    [SwaggerResponse(200, "Success - New Avatar URL")]
    [SwaggerResponse(400, "Bad Request")]
    public async Task<IActionResult> UploadAvatar([FromForm][SwaggerRequestBody(Required = true)] MediaUploadData data)
    {
        if (data.Media?.ContentType.StartsWith("image/") != true)
        {
            return BadRequest("Media must be an image.");
        }

        await using var file = data.Media.OpenReadStream();
        var newUrl = await users.UpdateUserAvatar(HttpContext.UserId(), (file, data.Media.ContentType));

        return Ok(newUrl);
    }

    /// <summary>
    /// Remove an avatar.
    /// </summary>
    [HttpDelete("avatar")]
    [ZitadelAuthorize]
    [SwaggerOperation(Description =
        "Remove the current avatar picture (if any) for the actual authenticated user.")]
    [SwaggerResponse(204, "Success - No Content")]
    public async Task<IActionResult> DeleteAvatar()
    {
        await users.UpdateUserAvatar(HttpContext.UserId());
        return NoContent();
    }

    /// <summary>
    /// Fetch a list of followers.
    /// </summary>
    [HttpGet("{id}/followers")]
    [SwaggerOperation(Description =
        " Fetch a (paginated) list of followers for a given user " +
        "(All users that follow the given user). " +
        "Returns only public information (public profile).")]
    [SwaggerResponse(200, "Success", typeof(PaginatedResult<User>))]
    public async Task<IActionResult> GetFollowers(string id, [FromQuery] PaginationParameters pagination)
    {
        var (followers, total) = await users.GetPaginatedFollowers(id, pagination);
        var loggedIn = User.Identity?.IsAuthenticated == true;

        var next = total > pagination.Offset + pagination.Limit
            ? $"{Url.ActionLink()}{(pagination with { Offset = pagination.Offset + pagination.Limit }).ToQueryString()}"
            : null;
        var prev = pagination.Offset > 0
            ? $"{Url.ActionLink()}{(pagination with { Offset = Math.Max(pagination.Offset - pagination.Limit, 0) }).ToQueryString()}"
            : null;

        return loggedIn
            ? Ok(new PaginatedResult<User>
            {
                Count = Convert.ToUInt32(total),
                Data = followers.Select(Models.User.FromEntity).ToList(),
                Next = next,
                Previous = prev,
            })
            : Ok(new PaginatedResult<PublicUser>
            {
                Count = Convert.ToUInt32(total),
                Data = followers.Select(PublicUser.FromEntity).ToList(),
                Next = next,
                Previous = prev,
            });
    }

    /// <summary>
    /// Fetch a list of followees.
    /// </summary>
    [HttpGet("{id}/followees")]
    [SwaggerOperation(Description =
        "Fetch a (paginated) list of followees for a given user" +
        "(All users that are being followed by the given user). " +
        "Returns only public information (public profile).")]
    [SwaggerResponse(200, "Success", typeof(PaginatedResult<User>))]
    public async Task<IActionResult> GetFollowees(string id, [FromQuery] PaginationParameters pagination)
    {
        var (followees, total) = await users.GetPaginatedFollowees(id, pagination);
        var loggedIn = User.Identity?.IsAuthenticated == true;

        var next = total > pagination.Offset + pagination.Limit
            ? $"{Url.ActionLink()}{(pagination with { Offset = pagination.Offset + pagination.Limit }).ToQueryString()}"
            : null;
        var prev = pagination.Offset > 0
            ? $"{Url.ActionLink()}{(pagination with { Offset = Math.Max(pagination.Offset - pagination.Limit, 0) }).ToQueryString()}"
            : null;

        return loggedIn
            ? Ok(new PaginatedResult<User>
            {
                Count = Convert.ToUInt32(total),
                Data = followees.Select(Models.User.FromEntity).ToList(),
                Next = next,
                Previous = prev,
            })
            : Ok(new PaginatedResult<PublicUser>
            {
                Count = Convert.ToUInt32(total),
                Data = followees.Select(PublicUser.FromEntity).ToList(),
                Next = next,
                Previous = prev,
            });
    }

    /// <summary>
    /// Follow a user.
    /// </summary>
    [HttpPut("{id}/followers")]
    [ZitadelAuthorize]
    [SwaggerOperation(Description =
        "Add the currently authenticated user as a follower to the given user (id). " +
        "User A (authenticated) calls this endpoint to follow user B (id).")]
    [SwaggerResponse(204, "Success - No Content")]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> FollowUser(string id)
    {
        try
        {
            await users.FollowUser(HttpContext.UserId(), id);
            return NoContent();
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Unfollow a user.
    /// </summary>
    [HttpDelete("{id}/followers")]
    [ZitadelAuthorize]
    [SwaggerOperation(Description =
        "Remove the currently authenticated user from the follower list of the given user (id)." +
        " User A (authenticated) calls this endpoint to unfollow user B (id).")]
    [SwaggerResponse(204, "Success - No Content")]
    public async Task<IActionResult> UnfollowUser(string id)
    {
        try
        {
            await users.UnfollowUser(HttpContext.UserId(), id);
            return NoContent();
        }
        catch (UserNotFoundException)
        {
            return NotFound();
        }
    }
}
