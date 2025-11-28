using Microsoft.AspNetCore.Authorization;

using Zitadel.Authentication;

namespace MumbleApi.Authentication;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ZitadelAuthorizeAttribute : AuthorizeAttribute;
