using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace MumbleApi.Models;

/// <summary>
/// Model to update user data.
/// </summary>
public class UpdateUserData
{
    /// <summary>
    /// If set, updates the firstname of the profile.
    /// </summary>
    [FromBody]
    [SwaggerSchema(WriteOnly = true)]
    public string? Firstname { get; set; }

    /// <summary>
    /// If set, updates the lastname of the profile.
    /// </summary>
    [FromBody]
    [SwaggerSchema(WriteOnly = true)]
    public string? Lastname { get; set; }

    /// <summary>
    /// If set, updates the username of the profile.
    /// </summary>
    [FromBody]
    [SwaggerSchema(WriteOnly = true)]
    public string? Username { get; set; }
}
