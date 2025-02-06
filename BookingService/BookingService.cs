//using AvailabilityService;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Net.Http;
using System.Text;

namespace BookingService
{
  public class BookingService
  {
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly bool _isTracingEnabled;
    private readonly HttpClient _httpClient;

    public BookingService(bool isTracingEnabled, HttpClient httpClient)
    {
      _isTracingEnabled = isTracingEnabled;
      var factory = new ConnectionFactory() { HostName = "localhost" };
      _connection = factory.CreateConnection();
      _channel = _connection.CreateModel();
      _httpClient = httpClient;
    }

    public async Task MakeBookingAsync(string user, string service)
    {
      if (_isTracingEnabled)
      {
        //using (var activity = _activitySource.StartActivity("SendBookingMessage", ActivityKind.Internal, Activity.Current.Context))
        //{
        //  activity?.SetTag("component", "rabbitmq");

        //  // Get the current Activity (trace) context
        //  var traceparent = activity?.Id ?? string.Empty;

        //  var properties = _channel.CreateBasicProperties();
        //  // Ensure headers dictionary is initialized if it's null
        //  if (properties.Headers == null)
        //  {
        //    properties.Headers = new Dictionary<string, object>();
        //  }
        //  // Add the trace context (traceparent) to the headers for trace propagation
        //  properties.Headers["traceparent"] = activity?.Id ?? string.Empty;
        //  /////////////////
        //  var bookingRequest = $"Booking request from {user} for {service}.";
        //  _channel.BasicPublish(exchange: "",
        //                        routingKey: "bookingQueue",
        //                        basicProperties: null,
        //                        body: Encoding.UTF8.GetBytes(bookingRequest));

        //  activity?.SetTag("message", "Booking message sent");
        //}

      }
      else
      {
        // Simulate booking without tracing
        var bookingRequest = $"Booking request from {user} for {service}.";
        _channel.BasicPublish(exchange: "",
                             routingKey: "bookingQueue",
                             basicProperties: null,
                             body: Encoding.UTF8.GetBytes(bookingRequest));
        
        var response = await _httpClient.GetAsync("https://localhost:7036/api/availability");

        Console.WriteLine($"Sent: {bookingRequest}");
        
  }

    }
  }
}
