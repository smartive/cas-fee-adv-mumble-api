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
        // Generate random delay between 50ms and 3000ms
        var delay = Random.Shared.Next(50, 3001);
        await Task.Delay(delay);

        // Continue with result execution (sending response)
        await next();
    }

    public Task OnResultExecutedAsync(ResultExecutedContext context)
    {
        // No action needed after result is executed
        return Task.CompletedTask;
    }
}
