using Microsoft.Extensions.Configuration;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry;
using TraceLibrary;

namespace Infrastructure.TraceLibrary
{
  public class TraceService
  {
    private bool isTracingEnabled=false;
    private TracerProvider tracerProvider;
    public void AddCustomTrace(string serviceName, IConfiguration configuration)
    {
      var jaegerConfig = configuration.GetSection("Jaeger").Get<TraceOptions>();

      if (isTracingEnabled)
      {
        tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(serviceName)
            .AddAspNetCoreInstrumentation() // Automatically instruments ASP.NET Core requests
            .AddHttpClientInstrumentation() // Automatically instruments HTTP client requests
            .AddJaegerExporter(options =>
            {
              options.AgentHost = jaegerConfig.AgentHost;
              options.AgentPort = jaegerConfig.AgentPort;
            })
            .AddConsoleExporter()
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
            .Build();
      }
    }

    // Method to toggle tracing at runtime
    public void ToggleTracing(bool enable, string serviceName, IConfiguration configuration)
    {
      //isTracingEnabled = configuration.GetValue<bool>("TracingEnabled");
      if (enable && !isTracingEnabled)
      {
        isTracingEnabled = true;
        AddCustomTrace(serviceName, configuration);
      }
      else if (!enable && isTracingEnabled)
      {
        isTracingEnabled = false;
        tracerProvider?.Dispose();  // Dispose of the TracerProvider to stop tracing
      }
    }
    public bool IsTracingEnabled() => isTracingEnabled;
  }
}

