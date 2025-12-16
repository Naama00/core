using System.Diagnostics;

namespace LogMiddleware;

public class LogMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogger logger;


    public LogMiddleware(RequestDelegate next, ILogger<LogMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task Invoke(HttpContext c)
    {
  
        var sw = new Stopwatch();
        sw.Start();
        await next.Invoke(c);
        logger.LogDebug($"{c.Request.Path}.{c.Request.Method} took {sw.ElapsedMilliseconds}ms."
            + $" User: {c.User?.FindFirst("userId")?.Value ?? "unknown"}");
        if (!c.Response.HasStarted && c.Response.StatusCode >= 200 && c.Response.StatusCode != 204 && c.Response.StatusCode != 304)
        {
            await c.Response.WriteAsync("hello! ho! in the end it succeeded!\n");
        }
    }


}
    public static partial class MiddlewareExtensions
{
    public static IApplicationBuilder UseLogMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LogMiddleware>();
    }
}


