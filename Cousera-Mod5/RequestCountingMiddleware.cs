    public class RequestCountingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly Dictionary<string, int> _routeCounts = new();

    public RequestCountingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var route = context.Request.Path.Value ?? "/";

        // Increment the counter for this route
        // Variante 1: if/else
        if (_routeCounts.ContainsKey(route))
        {
            _routeCounts[route]++;
        }
        else
        {
            _routeCounts.Add(route, 1);
        }
        // Optionally add the count into a response header
        context.Response.OnStarting(() =>
        {
            if (_routeCounts.TryGetValue(route, out var count))
            {
                context.Response.Headers["X-Route-Call-Count"] = count.ToString();
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }

    // Public accessor if you want to expose the counts elsewhere
    public static IReadOnlyDictionary<string, int> Counts => _routeCounts;
}