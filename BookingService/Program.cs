using Infrastructure.TraceLibrary;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using Serilog;
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
builder.Services.AddInitialTraceConfiguration("new_custom", builder.Configuration);   //(1)

builder.Services.AddHttpClient();  // Registering HttpClient for DI

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

app.UseMiddleware<InstrumentationMiddleware>("new_custom", new List<string> { "UserId"}); // Add custom middleware

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
