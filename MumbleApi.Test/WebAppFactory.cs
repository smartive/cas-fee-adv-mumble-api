using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using MumbleApi.Database;
using MumbleApi.Services;
using MumbleApi.Test.Mocks;

using Zitadel.Authentication;
using Zitadel.Extensions;

namespace MumbleApi.Test;

public class WebAppFactory : WebApplicationFactory<Program>
{
    public async Task PrepareTestData(Func<DataContext, Task> action)
    {
        await using var scope = this.Services.CreateAsyncScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<DataContext>>();
        await using var context = await factory.CreateDbContextAsync();
        await context.Database.EnsureDeletedAsync();
        await context.Database.MigrateAsync();
        await action(context);
        await context.SaveChangesAsync();
    }

    public HttpClient CreateUnauthorizedClient()
    {
        var client = this.CreateClient();
        client.DefaultRequestHeaders.Add("x-zitadel-fake-auth", "false");
        return client;
    }

    public HttpClient CreateUserClient(string userId)
    {
        var client = this.CreateClient();
        client.DefaultRequestHeaders.Add("x-zitadel-fake-user-id", userId);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var testEnvs = new[]
        {
            ("DATABASE__HOST", "localhost"),
            ("DATABASE__DATABASE", "test"),
            ("DATABASE__USERNAME", "user"),
            ("DATABASE__PASSWORD", "pass"),
            ("AUTHENTICATION__JWTKEY", "{}"),
        };
        foreach (var (name, value) in testEnvs)
        {
            if (Environment.GetEnvironmentVariable(name) == null)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }

        base.ConfigureWebHost(builder);

        builder.UseSolutionRelativeContentRoot("MumbleApi");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IStorage>();
            services.AddScoped<IStorage, MockStorage>();

            services
                .AddAuthorization()
                .AddAuthentication(ZitadelDefaults.FakeAuthenticationScheme)
                .AddZitadelFake(o => o.FakeZitadelId = TestData.DefaultAuthUser.Id);
        });
        builder.ConfigureLogging(l => l.ClearProviders());
    }
}
