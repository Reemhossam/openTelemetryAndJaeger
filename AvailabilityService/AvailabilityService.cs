using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System;
using System.Text;
using System.Diagnostics;

namespace AvailabilityService
{
  public class AvailabilityService
  {
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly bool _isTracingEnabled;

    public AvailabilityService(bool isTracingEnabled)
    {
      _isTracingEnabled = isTracingEnabled;
      var factory = new ConnectionFactory() { HostName = "localhost" };
      _connection = factory.CreateConnection();
      _channel = _connection.CreateModel();
    }

    public void Start()
    {
      _channel.QueueDeclare(queue: "bookingQueue",
                           durable: false,
                           exclusive: false,
                           autoDelete: false,
                           arguments: null);
      var consumer = new EventingBasicConsumer(_channel);
      consumer.Received += (sender, args) =>
      {
        var body = args.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine($"Received: {message}");

        if (_isTracingEnabled)
        {

          //// Extract trace context from the message headers
          //if (args.BasicProperties.Headers != null && args.BasicProperties.Headers.ContainsKey("traceparent"))
          //{
          //  var traceparent = args.BasicProperties.Headers["traceparent"].ToString();
          //  var parentContext = new ActivityContext(ActivityTraceId.CreateFromString(traceparent), ActivitySpanId.CreateRandom(), ActivityTraceFlags.Recorded);

          //    using var activity = _activitySource.StartActivity("SendNotification");
          //  activity?.SetTag("component", "rabbitmq");
          //  // Simulate checking availability
          //  if (message.Contains("available"))
          //    {
          //      SendNotification(message);
          //    }

          //  // Optionally, set custom tags for this activity
          //  activity?.SetTag("message", message);
          //}
        }
        else
        {
          // Process without tracing
          if (message.Contains("available"))
          {
            SendNotification(message);
          }
        }
      };

      _channel.BasicConsume(queue: "bookingQueue",
                           autoAck: true,
                           consumer: consumer);

      Console.WriteLine("Availability Service started.");
    }
    public void SendNotification(string bookingDetails)
    {
      // Simulate sending notification (e.g., email, SMS)
      Console.WriteLine($"Notification sent for booking: {bookingDetails}");
    }
  }

}
