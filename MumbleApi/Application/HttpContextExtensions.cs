using System.Security.Claims;

using MumbleApi.Errors;

using Zitadel.Authentication;

namespace MumbleApi.Application;

public static class HttpContextExtensions
{
    public static string? OptionalUserId(this HttpContext context) =>
        context.User.FindFirstValue(OidcClaimTypes.Subject);

    public static string UserId(this HttpContext context) => context.User.FindFirstValue(OidcClaimTypes.Subject) ??
                                                             throw new UserNotFoundException("No UserID Found.");
}
