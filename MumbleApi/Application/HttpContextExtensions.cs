using System.Security.Claims;

using Zitadel.Authentication;

namespace MumbleApi.Application;

public static class HttpContextExtensions
{
    public static string? OptionalUserId(this HttpContext context) =>
        context.User.FindFirstValue(OidcClaimTypes.Subject);

    public static string UserId(this HttpContext context) => context.User.FindFirstValue(OidcClaimTypes.Subject) ??
                                                             throw new Exception("No UserID Found.");
}
