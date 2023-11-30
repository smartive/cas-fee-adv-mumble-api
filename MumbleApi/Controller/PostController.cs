using Microsoft.AspNetCore.Mvc;

using MumbleApi.Application;
using MumbleApi.Authentication;
using MumbleApi.Errors;
using MumbleApi.Models;
using MumbleApi.Services;

using Swashbuckle.AspNetCore.Annotations;

using PostEntity = MumbleApi.Entities.Post;

namespace MumbleApi.Controller;

[ApiController]
[Route("posts")]
[Produces("application/json")]
[SwaggerTag("Manage posts in the Mumble system.")]
[OptionalZitadelAuthorize]
public class PostController(IPosts posts, IPostUpdates updates) : ControllerBase
{
    /// <summary>
    /// Fetch/Search a paginated list of posts.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Description =
        "Fetch or search a paginated list of posts, ordered by the time of their creation.")]
    [SwaggerResponse(200, "Success", typeof(PaginatedResult<Post>))]
    public async Task<IActionResult> Search([FromQuery] PostSearchParameters search)
    {
        var (dbPosts, total) = await posts.GetPaginatedPostsWithLikes(search);

        return Ok(new PaginatedResult<Post>
        {
            Count = Convert.ToUInt32(total),
            Data = dbPosts.Select(Post.FromEntity(HttpContext.OptionalUserId())).ToList(),
            Next = total > search.Offset + search.Limit
                ? $"{Url.ActionLink()}{(search with { Offset = search.Offset + search.Limit }).ToQueryString()}"
                : null,
            Previous = search.Offset > 0
                ? $"{Url.ActionLink()}{(search with { Offset = Math.Max(search.Offset - search.Limit, 0) }).ToQueryString()}"
                : null,
        });
    }

    /// <summary>
    /// Create a new post.
    /// </summary>
    [HttpPost]
    [ZitadelAuthorize]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 2 * 1024 * 1024)]
    [SwaggerOperation(Description =
        "Create a new post with the logged in user. " +
        "A post can contain text and/or an image. " +
        "Either text or the image must be provided, " +
        "otherwise a BadRequest is returned. Upload Limit: 2 MB.")]
    [SwaggerResponse(200, "Success - Created a new Post", typeof(Post))]
    [SwaggerResponse(400, "Bad Request")]
    public async Task<IActionResult> Create([FromForm][SwaggerRequestBody(Required = true)] CreatePostData data)
    {
        if (data.Media is not null && !data.Media.ContentType.StartsWith("image/"))
        {
            return BadRequest("Media must be an image.");
        }

        try
        {
            var userId = HttpContext.UserId();
            PostEntity postEntity;
            if (data.Media is not null)
            {
                await using var file = data.Media.OpenReadStream();
                postEntity = await posts.CreatePost(
                    userId,
                    text: data.Text,
                    media: (file, data.Media.ContentType));
            }
            else
            {
                postEntity = await posts.CreatePost(
                    userId,
                    text: data.Text);
            }

            await updates.NewPost(Post.FromEntity(postEntity));

            return Ok(Post.FromEntity(postEntity, userId));
        }
        catch (PostInvalidException)
        {
            return BadRequest("Post data is not valid.");
        }
    }

    /// <summary>
    /// Fetch a specific post with the given ID.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerResponse(200, "Success", typeof(Post))]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> GetById(Ulid id)
    {
        var post = await posts.GetPostById(id);
        if (post is null)
        {
            return NotFound();
        }

        return Ok(Post.FromEntity(post, HttpContext.OptionalUserId()));
    }

    /// <summary>
    /// Update a post with the given ID.
    /// </summary>
    [HttpPut("{id}")]
    [ZitadelAuthorize]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 2 * 1024 * 1024)]
    [SwaggerOperation(Description =
        "Update (replace) a post. This replaces the entire post. " +
        "If the post does not exist, a NotFound is returned. " +
        "If the post is not valid (has no text and no media), a BadRequest is returned. " +
        "There is no partial update on this method.")]
    [SwaggerResponse(200, "Success - Replaced the Post", typeof(Post))]
    [SwaggerResponse(400, "Bad Request")]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> Replace(
        Ulid id,
        [FromForm] [SwaggerRequestBody(Required = true)]
        CreatePostData data)
    {
        if (data.Media is not null && !data.Media.ContentType.StartsWith("image/"))
        {
            return BadRequest("Media must be an image.");
        }

        try
        {
            PostEntity postEntity;
            var userId = HttpContext.UserId();
            if (data.Media is not null)
            {
                await using var file = data.Media.OpenReadStream();
                postEntity = await posts.ReplacePost(
                    userId,
                    id,
                    data.Text,
                    (file, data.Media.ContentType));
            }
            else
            {
                postEntity = await posts.ReplacePost(
                    userId,
                    id,
                    data.Text);
            }

            await updates.PostUpdated(Post.FromEntity(postEntity));

            return Ok(Post.FromEntity(postEntity, userId));
        }
        catch (PostNotFoundException)
        {
            return NotFound();
        }
        catch (PostInvalidException)
        {
            return BadRequest("Post data is not valid.");
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Partial update of a post.
    /// </summary>
    [HttpPatch("{id}")]
    [ZitadelAuthorize]
    [SwaggerOperation(Description =
        "This partially updates (patches) a post with the given ID. " +
        "All fields that are \"null\" are ignored and not updated. " +
        "If the post text is set to an empty string, it will be removed (null). " +
        "The post must always have either text or media. " +
        "If this patch results in a post with no text and no media, a BadRequest is returned.")]
    [SwaggerResponse(204, "Success - No Content")]
    [SwaggerResponse(400, "Bad Request")]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> Update(
        Ulid id,
        [FromBody] [SwaggerRequestBody(Required = true)]
        UpdatePostData data)
    {
        try
        {
            if (data.Text is not null)
            {
                var userId = HttpContext.UserId();
                var post = await posts.UpdatePost(userId, id, data.Text);
                await updates.PostUpdated(Post.FromEntity(post));
            }
        }
        catch (PostNotFoundException)
        {
            return NotFound(id);
        }
        catch (PostInvalidException)
        {
            return BadRequest("Post data is not valid");
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a given post.
    /// </summary>
    [HttpDelete("{id}")]
    [ZitadelAuthorize]
    [SwaggerOperation(Description =
        "This can be a post or a reply. " +
        "The post is marked as deleted and will not be returned in any search.")]
    [SwaggerResponse(204, "Success - No Content")]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> Delete(Ulid id)
    {
        try
        {
            var userId = HttpContext.UserId();
            await posts.DeletePost(userId, id);
            await updates.PostDeleted(id);
        }
        catch (PostNotFoundException)
        {
            return NotFound();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }

        return NoContent();
    }

    /// <summary>
    /// Update the media of a post.
    /// </summary>
    [HttpPut("{id}/media")]
    [ZitadelAuthorize]
    [Produces("text/plain")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 2 * 1024 * 1024)]
    [SwaggerOperation(Description =
        "This replaces the entire media of the post and returns the new media URL.")]
    [SwaggerResponse(200, "Success", typeof(string))]
    [SwaggerResponse(400, "Bad Request")]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> UpdateMedia(
        Ulid id,
        [FromForm] [SwaggerRequestBody(Required = true)]
        MediaUploadData uploadData)
    {
        if (uploadData.Media?.ContentType.StartsWith("image/") != true)
        {
            return BadRequest("Media must be an image.");
        }

        try
        {
            await using var file = uploadData.Media.OpenReadStream();
            var userId = HttpContext.UserId();
            var post = await posts.UpdatePostMedia(userId, id, (file, uploadData.Media.ContentType));
            await updates.PostUpdated(Post.FromEntity(post));

            return Ok(post.MediaUrl);
        }
        catch (PostNotFoundException)
        {
            return NotFound();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }
    }

    /// <summary>
    /// Remove the media of a post.
    /// </summary>
    [HttpDelete("{id}/media")]
    [ZitadelAuthorize]
    [SwaggerOperation(Description =
        "Remove the media of a post with the given ID. " +
        "The post must always have either text or media. " +
        "If this delete results in a post with no text and no media, a BadRequest is returned.")]
    [SwaggerResponse(204, "Success - No Content")]
    [SwaggerResponse(400, "Bad Request")]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> DeleteMedia(Ulid id)
    {
        try
        {
            var userId = HttpContext.UserId();
            var post = await posts.UpdatePostMedia(userId, id);
            await updates.PostUpdated(Post.FromEntity(post));
        }
        catch (PostInvalidException)
        {
            return BadRequest("Post data is not valid.");
        }
        catch (PostNotFoundException)
        {
            return NotFound();
        }
        catch (ForbiddenException)
        {
            return Forbid();
        }

        return NoContent();
    }

    /// <summary>
    /// Fetch a list of replies.
    /// </summary>
    [HttpGet("{id}/replies")]
    [SwaggerOperation(Description =
        "Fetch a list of (paginated) replies for a given post. " +
        "There is only one level for replies. Replies cannot have replies. " +
        "Trying to fetch replies for a reply will result in a BadRequest.")]
    [SwaggerResponse(200, "Success", typeof(PaginatedResult<Reply>))]
    [SwaggerResponse(400, "Bad Request")]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> GetReplies(Ulid id, [FromQuery] PaginationParameters pagination)
    {
        try
        {
            var (replies, total) = await posts.GetPaginatedReplies(id, pagination);
            return Ok(new PaginatedResult<Reply>
            {
                Count = Convert.ToUInt32(total),
                Data = replies.Select(Reply.FromEntity(HttpContext.OptionalUserId())).ToList(),
                Next = total > pagination.Offset + pagination.Limit
                    ? $"{Url.ActionLink()}{(pagination with { Offset = pagination.Offset + pagination.Limit }).ToQueryString()}"
                    : null,
                Previous = pagination.Offset > 0
                    ? $"{Url.ActionLink()}{(pagination with { Offset = Math.Max(pagination.Offset - pagination.Limit, 0) }).ToQueryString()}"
                    : null,
            });
        }
        catch (PostNotFoundException)
        {
            return NotFound();
        }
        catch (PostIsAReplyException)
        {
            return BadRequest("Post is a reply and cannot have replies.");
        }
    }

    /// <summary>
    /// Create a reply to a post.
    /// </summary>
    [HttpPost("{id}/replies")]
    [ZitadelAuthorize]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 2 * 1024 * 1024)]
    [SwaggerOperation(Description =
        "Create a new reply to a post. " +
        "It is not possible to create a reply to a reply. " +
        "This results in a BadRequest.")]
    [SwaggerResponse(200, "Success", typeof(Reply))]
    [SwaggerResponse(400, "Bad Request")]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> CreateReply(
        Ulid id,
        [FromForm] [SwaggerRequestBody(Required = true)]
        CreatePostData data)
    {
        if (data.Media is not null && !data.Media.ContentType.StartsWith("image/"))
        {
            return BadRequest("Media must be an image.");
        }

        try
        {
            var userId = HttpContext.UserId();
            PostEntity postEntity;
            if (data.Media is not null)
            {
                await using var file = data.Media.OpenReadStream();
                postEntity = await posts.CreatePost(
                    userId,
                    id,
                    data.Text,
                    (file, data.Media.ContentType));
            }
            else
            {
                postEntity = await posts.CreatePost(
                    userId,
                    id,
                    data.Text);
            }

            await updates.NewPost(Reply.FromEntity(postEntity));

            return Ok(Reply.FromEntity(postEntity, userId));
        }
        catch (PostInvalidException)
        {
            return BadRequest("Post data is not valid.");
        }
        catch (PostIsAReplyException)
        {
            return BadRequest("Post is a reply and cannot have replies.");
        }
        catch (PostNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Like a post.
    /// </summary>
    [HttpPut("{id}/likes")]
    [ZitadelAuthorize]
    [SwaggerOperation(Description =
        " A user can only like a post once. " +
        "This is an idempotent operation. If the user already likes the post, " +
        "nothing is updated.")]
    [SwaggerResponse(204, "Success - No Content")]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> Like(Ulid id)
    {
        try
        {
            var userId = HttpContext.UserId();
            if (await posts.LikePost(userId, id))
            {
                await updates.PostLiked(userId, id);
            }

            return NoContent();
        }
        catch (PostNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Unlike a post.
    /// </summary>
    [HttpDelete("{id}/likes")]
    [ZitadelAuthorize]
    [SwaggerOperation(Description =
        "Unlike the given post for the current user. " +
        "If the user does not like the post, nothing is updated.")]
    [SwaggerResponse(204, "Success - No Content")]
    [SwaggerResponse(404, "Not Found")]
    public async Task<IActionResult> Unlike(Ulid id)
    {
        try
        {
            var userId = HttpContext.UserId();
            if (await posts.UnlikePost(userId, id))
            {
                await updates.PostUnliked(userId, id);
            }

            return NoContent();
        }
        catch (PostNotFoundException)
        {
            return NotFound();
        }
    }
}
