using Microsoft.AspNetCore.Mvc;

namespace MumbleApi.Models;

public record PaginationParameters
{
    /// <summary>
    /// The offset for pagination of further calls. Defaults to 0 if omitted.
    /// </summary>
    /// <example>0</example>
    [FromQuery(Name = "offset")]
    public int Offset { get; set; }

    /// <summary>
    /// The limit of items to return. Minimum is 1, maximum is 1000.
    /// Defaults to 100.
    /// </summary>
    /// <example>100</example>
    [FromQuery(Name = "limit")]
    public int Limit { get; set; } = 100;

    public virtual QueryString ToQueryString() => QueryString.Create(new[]
    {
        new KeyValuePair<string, string?>("offset", Offset.ToString()),
        new KeyValuePair<string, string?>("limit", Limit.ToString()),
    });
}
