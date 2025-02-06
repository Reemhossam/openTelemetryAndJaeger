using Infrastructure.TraceLibrary;
using RabbitMQ.Client;
using Serilog;
using TraceLibrary;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



//// Add services to the container.
//builder.Services.AddOpenTelemetry()
//    .WithTracing(builder => builder
//        .AddSource("Custom.Middleware") // Match the ActivitySource name
//        .AddAspNetCoreInstrumentation(opt =>
//                                      {
//                                        opt.EnrichWithHttpRequest = (activity, httpRequest) => activity.SetBaggage("UserId", "1234");
//                                      }
//        )
//        .AddHttpClientInstrumentation(options =>
//         {
//        //// Custom logic to add tags/attributes to the HTTP client spans
//        //options.EnrichWithHttpRequestMessage = (activity, request) =>
//        //{
//        //  // Extract UserId from the request headers (if it exists)
//        //  var userId = request.Headers.FirstOrDefault(x => x.Key == "UserId").Value?.FirstOrDefault(); ;

//        //  // Check if UserId is found in headers, then add it as a tag
//        //  if (!string.IsNullOrEmpty(userId))
//        //  {
//        //    activity.SetTag("userTag", userId);  // Add custom tag to the span
//        //  }
//        //  else
//        //  {
//        //    activity.SetTag("userTag", "unknown");  // Set a default value if not found
//        //  }
//        //};
//        })

//        //.AddSqlClientInstrumentation()
//        .AddConsoleExporter()
//        .AddJaegerExporter()
//        //.AddSource("BookingSource")
//        .SetResourceBuilder(
//            ResourceBuilder.CreateDefault()
//                //.AddService(serviceName: "BookingService")))
//                //.AddService(serviceName: "BookingServiceSpan")))
//                .AddService(serviceName: "Custom.Middleware")))
//    .StartWithHost();



// Add RabbitMQ Client (if needed for communication)
builder.Services.AddSingleton<IModel>(sp =>
{
  var factory = new ConnectionFactory() { HostName = "localhost" };
  var connection = factory.CreateConnection();
  return connection.CreateModel();
});
//builder.Services.AddCustomTrace("new_custom", builder.Configuration);
var tracingManager = new TraceService();
tracingManager.AddCustomTrace("new_custom", builder.Configuration);
builder.Services.AddSingleton<TraceService>();

builder.Services.AddHttpClient();  // Registering HttpClient for DI

//////////////////////////////////
// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

//app.UseMiddleware<CustomInstrumentationMiddleware>(); // Add custom middleware
// Use the custom telemetry middleware with dynamic values
//var tags = new List<string> { "UserId", "UserId1", "UserId2" };
app.UseMiddleware<InstrumentationMiddleware>("new_custom", new List<string> { "UserId", "UserId1", "UserId2" }); // Add custom middleware

// Example: Toggle tracing dynamically via an API endpoint
app.MapGet("/toggleTracing", (TraceService traceService) =>
{
  bool isTracingEnabled = traceService.IsTracingEnabled();
  traceService.ToggleTracing(!isTracingEnabled, "new_custom", builder.Configuration); // Toggle the state
  return Task.FromResult(isTracingEnabled ? "Tracing disabled" : "Tracing enabled");
});

// Configure the HTTP request pipeline
app.UseSerilogRequestLogging();


app.MapControllers();

app.Run();
