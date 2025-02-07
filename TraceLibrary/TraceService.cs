using Microsoft.Extensions.Configuration;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TraceLibrary;

namespace Infrastructure.TraceLibrary
{
  public class TraceService
  {
    private static TraceService? _instance; // Singleton instance
    private bool _isTracingEnabled;
    private TracerProvider? _tracerProvider;
    private readonly TraceOptions? _jaegerConfig;

    private TraceService(IConfiguration configuration)
    {
      _jaegerConfig = configuration.GetSection("Jaeger").Get<TraceOptions>();
      _isTracingEnabled = configuration.GetValue<bool>("TracingEnabled", false);
    }

    public static TraceService GetInstance(IConfiguration configuration)
    {
      if (_instance == null)
      {
        _instance = new TraceService(configuration);
      }
      return _instance;
    }
    public void AddTraceConfiguration(string serviceName)
    {
      if (_isTracingEnabled)
      {
        _tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(serviceName)
            .AddAspNetCoreInstrumentation() // Automatically instruments ASP.NET Core requests
            .AddHttpClientInstrumentation() // Automatically instruments HTTP client requests
            .AddJaegerExporter(options =>
            {
              options.AgentHost = _jaegerConfig.AgentHost;
              options.AgentPort = _jaegerConfig.AgentPort;
            })
            .AddConsoleExporter()   // Console exporter (can be removed in production)
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
            .Build();
      }
    }

    // Method to toggle tracing at runtime
    public void ToggleTracing(bool enable, string serviceName, IConfiguration configuration)
    {
      if (enable && !_isTracingEnabled)
      {
        _isTracingEnabled = true;
        AddTraceConfiguration(serviceName);
      }
      else if (!enable && _isTracingEnabled)
      {
        _isTracingEnabled = false;
        _tracerProvider?.Dispose();  // Dispose of the TracerProvider to stop tracing
      }
    }
    public bool IsTracingEnabled() => _isTracingEnabled;
  }
}

