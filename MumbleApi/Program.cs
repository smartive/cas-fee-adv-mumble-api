using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

using Community.Microsoft.Extensions.Caching.PostgreSql;

using Lib.AspNetCore.ServerSentEvents;

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using MumbleApi.Application;
using MumbleApi.Database;
using MumbleApi.OpenApi;
using MumbleApi.Services;

using Zitadel.Authentication;
using Zitadel.Extensions;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
#if DEBUG
    .AddJsonFile("appsettings.Secrets.json", true)
    .Build()
#endif
    .Get<AppConfig>() ?? throw new("Could not read config.");
builder.Services.AddSingleton(config);

builder.Services.AddScoped<IPosts, Posts>();
builder.Services.AddScoped<IStorage, Storage>();
builder.Services.AddScoped<IUsers, Users>();

builder.Services
    .AddDbContextFactory<DataContext>(
        db => db

#if DEBUG
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
#endif
            .UseNpgsql(config.Database.ConnectionString));
builder.Services.AddDistributedPostgreSqlCache(o =>
{
    o.ConnectionString = config.Database.ConnectionString;
    o.SchemaName = config.Database.Schema;
    o.TableName = "zitadel_introspection_cache";
});

builder.Services
    .AddAuthorization()
    .AddAuthentication(ZitadelDefaults.AuthenticationScheme)
    .AddZitadelIntrospection(o =>
    {
        o.Authority = config.Authentication.Issuer;
        o.JwtProfile = config.Authentication.ApplicationCredentials;
        o.EnableCaching = true;
        o.CacheDuration = TimeSpan.FromHours(6);
        o.Events.OnTokenValidated += async context =>
        {
            var userId = context.Principal?.FindFirstValue(OidcClaimTypes.Subject);
            if (userId is null)
            {
                return;
            }

            var factory = context.HttpContext.RequestServices.GetRequiredService<IDbContextFactory<DataContext>>();
            await using var db = await factory.CreateDbContextAsync();
            await db.Database.ExecuteSqlInterpolatedAsync(
                @$"insert into users (id, username, firstname, lastname) 
                     values (
                        {userId},
                        {context.Principal?.FindFirstValue(OidcClaimTypes.Username)},
                        {context.Principal?.FindFirstValue(OidcClaimTypes.GivenName)},
                        {context.Principal?.FindFirstValue(OidcClaimTypes.FamilyName)}
                     ) on conflict do nothing;");
        };
    });

builder.Services.AddHealthChecks();
builder.Services.AddServerSentEvents<IPostUpdates, PostUpdates>();
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Services.AddCors(
    o => o.AddDefaultPolicy(
        p => p
            .AllowAnyMethod()
            .SetPreflightMaxAge(TimeSpan.FromHours(1))
            .AllowAnyHeader()
            .AllowAnyOrigin()));
builder.Services.AddSwaggerGen(o =>
{
    o.EnableAnnotations();
    o.SwaggerDoc("v1", new()
    {
        Version = "v1",
        Title = "Mumble API",
        Description =
            "API for 'mumble'. A simple messaging/twitter like API for the CAS Frontend Engineering Advanced.",
        Contact = new() { Name = "smartive AG", Email = "hello@smartive.ch", Url = new("https://smartive.ch"), },
        License = new() { Name = "Apache 2.0", Url = new("https://www.apache.org/licenses/LICENSE-2.0"), },
    });

    o.AddSecurityDefinition(ZitadelDefaults.AuthenticationScheme, new()
    {
        Name = ZitadelDefaults.AuthenticationScheme,
        Description = "ZITADEL OpenID Connect Login (OAuth Introspection)",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.OpenIdConnect,
        OpenIdConnectUrl = new($"{config.Authentication.Issuer}/.well-known/openid-configuration"),
        BearerFormat = "opaque",
        Scheme = "Bearer",
    });
    o.AddSecurityDefinition("None", new()
    {
        Name = "None",
        Description = "No Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = string.Empty,
    });

    o.MapType<Ulid>(() => new OpenApiSchema
    {
        Format = "ULID",
        Title = "ULID",
        Description = "Universally Unique Lexicographically Sortable Identifier",
        Type = "string",
        Example = new OpenApiString("01GEESHPQQ4NJKNZJN9AKWQW6G"),
        ExternalDocs = new() { Description = "ULID Specification", Url = new("https://github.com/ulid/spec"), },
    });

    o.OperationFilter<InternalServerErrorResponseFilter>();
    o.OperationFilter<SecurityRequirementFilter>();
    o.DocumentFilter<ServerSentEventFilter>();

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DataContext>();
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseForwardedHeaders(
        new()
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto,
            KnownNetworks = { new IPNetwork(IPAddress.Parse("0.0.0.0"), 0) },
        });
    app.UseHsts();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors();
app.MapControllers();
app.MapServerSentEvents<PostUpdates>("/posts/_sse");
app.MapHealthChecks("/healthz");

app.UseSwagger();
app.UseSwaggerUI(o =>
{
    o.DocumentTitle = "Mumble API";
    o.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    o.RoutePrefix = string.Empty;
    o.OAuthConfigObject = new()
    {
        Scopes = new List<string> { "openid", "profile", "email" },
        ClientId = config.Swagger.ClientId,
        UsePkceWithAuthorizationCodeGrant = true,
    };
    o.EnablePersistAuthorization();
});

await app.RunAsync();

public partial class Program
{
    protected Program()
    {
    }
}
