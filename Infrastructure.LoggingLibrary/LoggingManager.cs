using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Infrastructure.LoggingLibrary
{
  public class LoggingManager
  {
    private static ILogger _logger;
    private readonly IConfiguration _configuration;

    public LoggingManager(IConfiguration configuration)
    {
      _configuration = configuration; 

      var seqUrl = _configuration.GetValue<string>("Logging:SeqUrl");
      _logger = new LoggerConfiguration()
      .MinimumLevel.Information()  // Default log level
      .WriteTo.Seq(seqUrl)
      .CreateLogger();

      Log.Logger = _logger;
    }

    // Method to dynamically change the log level for a specific class
    public void SetLogLevel(string className, string level)
    {
      var seqUrl = _configuration.GetValue<string>("Logging:SeqUrl");
      if (Enum.TryParse<LogEventLevel>(level, true, out var logLevel))
      {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Information()  // Default log level for everything
            .MinimumLevel.Override(className, logLevel)  // Override log level for specific class
            .WriteTo.Seq(seqUrl)
            .CreateLogger();

        // Reconfigure the logger with the new settings
        Log.Logger = _logger;
      }
    }

    // For easier logging from any class
    public static ILogger ForContext<T>()
    {
      return Log.ForContext<T>();
    }
  }
}
