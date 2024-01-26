using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace MumbleApi.Models;

/// <summary>
/// Data that is used to upload media.
/// </summary>
public class MediaUploadData
{
    /// <summary>
    /// Media file for the upload.
    /// </summary>
    [FromForm(Name = "media")]
    [SwaggerSchema(WriteOnly = true, Required = ["media"])]
    public IFormFile? Media { get; set; }
}
