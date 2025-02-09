using Infrastructure.LoggingLibrary;

public class MyService
{
  private readonly Serilog.ILogger _logger;

  public MyService()
  {
    _logger = LoggingManager.ForContext<MyService>();
  }

  public void Execute()
  {
    _logger.Information("Executing service logic...");
    _logger.Debug("This is a debug message.");
  }
}