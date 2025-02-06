using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AvailabilityService.Controllers
{
  [Route("api/availability")]
  [ApiController]
  public class AvailabilityController : ControllerBase
  {
    private readonly ActivitySource _activitySource;
    public AvailabilityController()
    {
      _activitySource = new ActivitySource("AvailabilityServiceSpan"); // Manually define the activity source for AvailabilityService
    }

    [HttpGet]
    public async Task AvailabilityCheck()
    {
      bool isTracingEnabled = true; // Toggle this to enable/disable tracing

      if (isTracingEnabled) {

        // Start a new span for handling the request in AvailabilityService
        using (var activity = _activitySource.StartActivity("GetAvailability", ActivityKind.Server))
        {
          //throw new Exception();
          // Add tags to the span for tracking
          activity?.SetTag("service", "AvailabilityService");
          

        }
      }
      else {
        throw new Exception();
        var availabilityService = new AvailabilityService(isTracingEnabled);
        availabilityService.Start();

        Console.WriteLine("Availability Service is now listening for booking requests.");
      }

      
    }

    //[HttpGet]
    //public IActionResult CheckAvailability()
    //{
    //  // Simulate availability check
    //  return Ok("Availability check passed");
    //}
  }
}
