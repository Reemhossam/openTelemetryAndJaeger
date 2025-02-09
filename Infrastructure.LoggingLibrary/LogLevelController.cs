using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.LoggingLibrary
{
  [Route("api/[controller]")]
  [ApiController]
  public class LogLevelController (LoggingManager loggingManager) : ControllerBase
  {
    [HttpPost("set-log-level")]
    public IActionResult SetLogLevel(string className, string level)
    {

      // Change log level for a specific class
      loggingManager.SetLogLevel(className, level);
      return Ok($"Log level for class {className} set to {level}");
    }
  }
}
