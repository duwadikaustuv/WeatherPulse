using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Serilog;
using WeatherPulse.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Serilog (Structured logging)
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(ctx.Configuration));

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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

app.UseRouting();

app.UseAuthorization();

// 6. Use CORS
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthorization();

// Hangfire Dashboard with Basic Authentication
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[]
    {
        new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            RequireSsl = false,
            SslRedirect = false,
            LoginCaseSensitive = true,
            Users = new []
            {
                new BasicAuthAuthorizationUser
                {
                    Login = "admin",
                    PasswordClear = "password123"
                }
            }
        })
    }
});

app.MapControllers();

app.Run();