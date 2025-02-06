using Infrastructure.TraceLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace TraceLibrary
{
  public static class TraceConfiguration
  {
    public static void AddCustomTrace(this IServiceCollection services, string serviceName, IConfiguration configuration)
    {
      var jaegerConfig = configuration.GetSection("Jaeger").Get<TraceOptions>();

      services.AddOpenTelemetry()
          .WithTracing(builder => builder
              .AddSource(serviceName) // Match the ActivitySource name
              .AddAspNetCoreInstrumentation() // Automatically instruments ASP.NET Core requests
              .AddHttpClientInstrumentation() // Automatically instruments HTTP client requests
              .AddConsoleExporter()
              .AddJaegerExporter(options =>
              {
                options.AgentHost = jaegerConfig.AgentHost;
                options.AgentPort = jaegerConfig.AgentPort;
              })
              .SetResourceBuilder(
                  ResourceBuilder.CreateDefault()
                      .AddService(serviceName: serviceName))); // Set the service name for your traces
                                                               // Register the TracingManager as a Singleton
      services.AddSingleton<TraceService>();
    }
  }
}
