using AvailabilityService.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BookingService.Controllers
{
  [Route("api/booking")]
  [ApiController]
  public class BookingController : ControllerBase
  {
    private readonly HttpClient _httpClient;
    private readonly ActivitySource _activitySource;
    private readonly ILogger<BookingController> _logger;
    public BookingController(HttpClient httpClient, ILogger<BookingController> logger)
    {
      _httpClient = httpClient;
      _logger = logger;

      _activitySource = new ActivitySource("BookingServiceSpan"); // Manually define the activity source for the BookingService
    }

    [HttpPost]
    public async Task BookingOrder(string user, string service)
    {
      bool isTracingEnabled = false; // Toggle this to enable/disable tracing

      if (isTracingEnabled)
      {
        using (var activity = _activitySource.StartActivity("CreateBooking"))
        {
          // Add some custom tags to the activity/span
          activity?.SetTag("booking.request", "Booking created");

          // Now, manually propagate the trace context as we call the AvailabilityService
          using (var availabilityActivity = _activitySource.StartActivity("CallAvailabilityService", ActivityKind.Client))
          {
            availabilityActivity?.SetTag("service", "AvailabilityService");

            // Send HTTP request to the AvailabilityService
            var response = await _httpClient.GetAsync("https://localhost:7036/api/availability"); // Assuming AvailabilityService is running on this port

            if (response.IsSuccessStatusCode)
            {
              availabilityActivity?.SetTag("http.status_code", response.StatusCode);
            }
            else
            {
              availabilityActivity?.SetTag("http.error", true); // Mark the error if the request fails
            }
          }
         }
        }
      else
      {
        /////////////////////////automatic////////////////////////////
        var bookingService = new BookingService(isTracingEnabled, _httpClient);
        _logger.LogInformation("Availability check for {Date} ({Duration} hours): {Available}",
            "6/2/2025", "from 2:00 PM", true ? "Available" : "Not Available");
        await bookingService.MakeBookingAsync(user, service);

        Console.WriteLine("Booking request sent.");
      }

    }
  }
}
