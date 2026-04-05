using System.Net;
using System.Text.Json;

namespace ECommerceApp.Api.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly Dictionary<string, Queue<DateTime>> _requestLog = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly object _lock = new();

    public RateLimitingMiddleware(
        RequestDelegate next, 
        ILogger<RateLimitingMiddleware> logger,
        int maxRequests = 100,
        int timeWindowMinutes = 1)
    {
        _next = next;
        _logger = logger;
        _maxRequests = maxRequests;
        _timeWindow = TimeSpan.FromMinutes(timeWindowMinutes);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.ToString();

        if (path.StartsWith("/api/auth"))
        {
            await _next(context);
            return;
        }

        var key = $"{clientIp}:{path}";

        lock (_lock)
        {
            if (!_requestLog.ContainsKey(key))
            {
                _requestLog[key] = new Queue<DateTime>();
            }

            var now = DateTime.UtcNow;
            var requests = _requestLog[key];

            while (requests.Count > 0 && now - requests.Peek() > _timeWindow)
            {
                requests.Dequeue();
            }

            if (requests.Count >= _maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for {ClientIp} on {Path}", clientIp, path);
                context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
                context.Response.Headers["Retry-After"] = ((int)_timeWindow.TotalSeconds).ToString();
                return;
            }

            requests.Enqueue(now);
        }

        await _next(context);
    }
}
