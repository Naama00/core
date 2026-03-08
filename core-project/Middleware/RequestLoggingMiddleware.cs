using System.Diagnostics;
using System.Threading.Channels;
using Library.Models;

namespace Library.Middleware
{
    /// <summary>
    /// Middleware שמתפוס כל בקשה ורושם אותה לתור עבור עיבוד asynchronous
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, Channel<LogEntry> logChannel)
        {
            // התחל מדידת הזמן
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // קרא לנקסט middleware
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // חלץ את המידע ממצב הבקשה
                var controllerName = context.GetRouteValue("controller")?.ToString() ?? "Unknown";
                var actionName = context.GetRouteValue("action")?.ToString() ?? "Unknown";
                var httpMethod = context.Request.Method;
                var statusCode = context.Response.StatusCode;
                var username = context.User?.Identity?.Name ?? null;

                // צור LogEntry
                var logEntry = new LogEntry
                {
                    StartTime = DateTime.Now.AddMilliseconds(-stopwatch.ElapsedMilliseconds),
                    ControllerName = controllerName,
                    ActionName = actionName,
                    Username = username,
                    DurationMs = stopwatch.ElapsedMilliseconds,
                    HttpMethod = httpMethod,
                    StatusCode = statusCode
                };

                // כתוב ליומן לתור (asynchronous - לא יחסום את ה-request)
                try
                {
                    await logChannel.Writer.WriteAsync(logEntry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing to log channel: {ex.Message}");
                }
            }
        }
    }
}
