using Infrastructure.TraceLibrary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TraceLibrary
{
  public static class TraceConfiguration
  {
    public static void AddInitialTraceConfiguration(this IServiceCollection services, string serviceName, IConfiguration configuration)
    {

      var traceService = TraceService.GetInstance(configuration);
      traceService.AddTraceConfiguration(serviceName);

    }
  }
}
