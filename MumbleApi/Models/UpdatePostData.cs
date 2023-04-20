using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace MumbleApi.Models;

/// <summary>
/// Data that is used to patch a given post.
/// </summary>
public class UpdatePostData
{
    /// <summary>
    /// Text for the post. If omitted, the text will not be updated.
    /// If set to empty string, the text will be removed.
    /// </summary>
    [FromBody]
    [SwaggerSchema(WriteOnly = true)]
    public string? Text { get; set; }
}
