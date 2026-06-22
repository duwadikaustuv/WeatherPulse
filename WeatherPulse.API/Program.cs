using Hangfire;
using Serilog;
using WeatherPulse.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .Enrich.WithCorrelationId()
    .ReadFrom.Configuration(ctx.Configuration));

// Add Controllers + Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Infrastructure Layer (Polly, Redis, Hangfire)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.Run();