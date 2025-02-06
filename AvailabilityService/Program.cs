using OpenTelemetry.Trace;
using RabbitMQ.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//builder.Services.AddHostedService<AvailabilityService>();
// Add OpenTelemetry Tracing

// Add services to the container.
builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddAspNetCoreInstrumentation(opt =>
        {
          opt.EnrichWithHttpRequest = (activity, httpRequest) => activity.SetBaggage("UserId", "1234");
        }
        )
        .AddHttpClientInstrumentation()
        //.AddSqlClientInstrumentation()
        .AddConsoleExporter()
        .AddJaegerExporter()
        //.AddSource("AvailabilitySource")
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                 .AddService(serviceName: "AvailabilityService")))
    //.AddService(serviceName: "AvailabilityServiceSpan")))
    .StartWithHost();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
