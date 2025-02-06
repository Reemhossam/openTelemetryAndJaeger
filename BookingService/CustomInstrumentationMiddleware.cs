using Azure.Core;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace BookingService
{
  public class CustomInstrumentationMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ActivitySource ActivitySource;

    public CustomInstrumentationMiddleware(RequestDelegate next)
    {
      _next = next;
      ActivitySource = new("Custom.Middleware");
    }

    public async Task InvokeAsync(HttpContext context)
    {
      // Start a new activity (span) for this request
      using var activity = ActivitySource.StartActivity("CustomMiddleware", ActivityKind.Server);
      try
      {
        // Add tags to the activity
        var userId = context.Request.Headers.FirstOrDefault(x => x.Key == "UserId").Value.FirstOrDefault();
        activity?.SetTag("http.method", context.Request.Method);
        activity?.SetTag("http.url", context.Request.Path);
        activity?.SetTag("custom.user_id", userId ?? "anonymous");
        await _next(context); // Process the request pipeline
        // Add response-related tags
        activity?.SetTag("http.status_code", context.Response.StatusCode);
      }
      catch (Exception ex)
      {
        // Record exceptions in the span
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.RecordException(ex);
        throw;
      }
    }
  }
}
