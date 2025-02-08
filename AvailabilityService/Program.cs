using OpenTelemetry.Trace;
using RabbitMQ.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using Infrastructure.TraceLibrary;
using TraceLibrary;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<TraceService>(provider =>
{
  return TraceService.GetInstance(builder.Configuration); // Using Singleton instance
});
builder.Services.AddInitialTraceConfiguration("Availability", builder.Configuration);   //(1)


builder.Services.AddSingleton<IModel>(sp =>
{
  var factory = new ConnectionFactory() { HostName = "localhost" };
  var connection = factory.CreateConnection();
  return connection.CreateModel();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseSwagger();
  app.UseSwaggerUI();
}

app.UseMiddleware<InstrumentationMiddleware>("Availability", new List<string> { "UserId" }); // Add custom middleware

app.MapGet("/toggleTracing", (TraceService traceService) =>
{
  bool isTracingEnabled = traceService.IsTracingEnabled();
  traceService.ToggleTracing(!isTracingEnabled, "Availability", builder.Configuration); // Toggle the state
  return Task.FromResult(isTracingEnabled ? "Tracing disabled" : "Tracing enabled");
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
