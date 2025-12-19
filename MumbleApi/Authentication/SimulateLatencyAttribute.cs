using Microsoft.AspNetCore.Mvc.Filters;

namespace MumbleApi.Authentication;

/// <summary>
/// Action filter that simulates network latency by adding a random delay
/// between 50ms and 3000ms before returning the response.
/// Used for educational purposes to simulate real-world load conditions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SimulateLatencyAttribute : Attribute, IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Allow tests or specific environments to disable simulated latency.
        if (Environment.GetEnvironmentVariable("DISABLE_SIMULATED_LATENCY")?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
        {
            await next();
            return;
        }

        // Generate random delay between 50ms and 3000ms
        var delay = Random.Shared.Next(50, 3001);
        await Task.Delay(delay);

        // Continue with result execution (sending response)
        await next();
    }

#pragma warning disable S2325 // Method can be made static - required by IAsyncResultFilter interface
    public Task OnResultExecutedAsync(ResultExecutedContext context)
    {
        // No action needed after result is executed
        return Task.CompletedTask;
    }
#pragma warning restore S2325
}
