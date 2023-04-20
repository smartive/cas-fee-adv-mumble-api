using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace MumbleApi.Models;

/// <summary>
/// Data that is used to create a new post.
/// </summary>
public class CreatePostData
{
    /// <summary>
    /// Text for the new post. If not set, a media file must be set.
    /// If both are omitted, the API will return a 400 Bad Request.
    /// </summary>
    [FromForm(Name = "text")]
    [SwaggerSchema(WriteOnly = true)]
    public string? Text { get; set; }

    /// <summary>
    /// Media file for the post. If not set, a text must be set.
    /// If both are omitted, the API will return a 400 Bad Request.
    /// </summary>
    [FromForm(Name = "media")]
    [SwaggerSchema(WriteOnly = true)]
    public IFormFile? Media { get; set; }
}
