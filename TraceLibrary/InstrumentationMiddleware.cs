using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace TraceLibrary
{
  public class InstrumentationMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ActivitySource _activitySource;
    private readonly List<string> _tags;
    public InstrumentationMiddleware(RequestDelegate next, string activitySource, List<string> tags)
    {
      _next = next;
      _activitySource = new(activitySource); 
      _tags = tags;
    }

    public async Task InvokeAsync(HttpContext context)
    {
      // Start a new activity (span) for this request
      string activityName = $"Request_{context.Request.Method}_{context.Request.Path}";
      using var activity = _activitySource.StartActivity(activityName, ActivityKind.Server);
      try
      {
        // Add tags to the activity
        foreach (var _tag in _tags)
        {
          var tagValue = context.Request.Headers.FirstOrDefault(x => x.Key == _tag).Value.FirstOrDefault();
          if (tagValue != null)
            activity?.SetTag(_tag, tagValue);
        }
        await _next(context); // Process the request pipeline
      }
      catch (Exception ex)
      {
        // Record exceptions in the span
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        throw;
      }
    }
  }
}
