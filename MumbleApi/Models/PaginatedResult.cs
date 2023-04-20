using Swashbuckle.AspNetCore.Annotations;

namespace MumbleApi.Models;

/// <summary>
/// Paginated API result that contains arbitrary
/// data and an optional link for the next / previous page.
/// </summary>
/// <typeparam name="TData">Type for the contained data.</typeparam>
public class PaginatedResult<TData>
{
    /// <summary>
    /// The total count of results.
    /// </summary>
    /// <example>1337</example>
    [SwaggerSchema(ReadOnly = true)]
    public uint Count { get; set; }

    /// <summary>
    /// The data for this given page.
    /// </summary>
    [SwaggerSchema(ReadOnly = true, Nullable = false)]
    public IReadOnlyList<TData> Data { get; set; } = Array.Empty<TData>();

    /// <summary>
    /// Link to next page. If this is null, there is no next page.
    /// The link will contain pagination information (offset, limit).
    /// If returned by a search, the link will not contain the search parameters.
    /// </summary>
    [SwaggerSchema(ReadOnly = true)]
    public string? Next { get; set; }

    /// <summary>
    /// Link to previous page. If this is null, there is no next page.
    /// The link will contain pagination information (offset, limit).
    /// If returned by a search, the link will not contain the search parameters.
    /// </summary>
    [SwaggerSchema(ReadOnly = true)]
    public string? Previous { get; set; }
}
