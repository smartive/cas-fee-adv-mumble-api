using Microsoft.AspNetCore.Mvc.Filters;

namespace MumbleApi.Authentication;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class OptionalZitadelAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // This is intentionally left blank.
        // The filter will trigger authorization, but will not fail if the user is not authenticated.
        // The idea is to allow the user to access the endpoint if they are authenticated, but not force them to.
        // They may still access, but the result may vary.
    }
}
