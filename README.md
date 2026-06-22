# WeatherPulse

WeatherPulse is a RESTful API aggregator designed for outdoor enthusiasts. It consolidates real-time weather data, air quality index (AQI), and ultraviolet (UV) radiation levels from multiple external sources into a single, unified "Outdoor Score". The system employs distributed caching to achieve sub-40 millisecond response times and background job processing for asynchronous image overlay tasks.

## Table of Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Environment Setup](#environment-setup)
  - [Configuration](#configuration)
  - [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [License](#license)

## Features

- **Parallel Asynchronous Fetching**: Leverages `Task.WhenAll` to concurrently retrieve weather, pollution, and UV data, reducing total latency by over 60% compared to sequential calls.
- **Resilience and Fault Tolerance**: Implements Polly policies with exponential backoff retries (3 attempts: 2s, 4s, 8s) to handle transient HTTP failures and rate limits.
- **Distributed Caching**: Utilizes Redis to cache aggregated results with a configurable 30-minute TTL, ensuring cached requests respond in under 40 milliseconds.
- **Background Job Processing**: Integrates Hangfire to manage long-running tasks, such as image processing, returning `202 Accepted` responses immediately.
- **Clean Architecture**: Enforces strict separation of concerns across Domain, Application, Infrastructure, and Presentation layers.
- **Observability**: Structured logging via Serilog with correlation IDs flowing through the entire request pipeline.

## Technology Stack

- **Runtime**: .NET 10
- **Framework**: ASP.NET Core Web API
- **Caching**: Redis (StackExchange.Redis)
- **Background Jobs**: Hangfire (PostgreSQL storage)
- **HTTP Resilience**: Polly
- **Logging**: Serilog
- **Object Storage**: AWS S3 SDK (for pre-signed URL generation)
- **API Documentation**: Swagger / OpenAPI
- **Containerization**: Docker Compose (Redis, PostgreSQL)

## Architecture

The solution is structured according to Clean Architecture principles to ensure maintainability and testability.

- **WeatherPulse.Domain**: Contains core entities, enums, and business logic interfaces.
- **WeatherPulse.Application**: Defines Data Transfer Objects (DTOs) and the application service contracts.
- **WeatherPulse.Infrastructure**: Implements external API clients, Polly retry policies, Redis caching, and Hangfire configurations.
- **WeatherPulse.API**: Hosts the REST controllers, middleware, and serves as the composition root.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for running Redis and PostgreSQL locally)
- [Visual Studio 2026](https://visualstudio.microsoft.com/) or any preferred C# IDE

### Environment Setup

Clone the repository and navigate to the solution root:

```bash
git clone https://github.com/your-username/weatherpulse.git
cd weatherpulse
```

Start the required local infrastructure using Docker Compose:

```bash
docker-compose up -d
```

This command initializes:

- Redis on `localhost:6379`
- PostgreSQL on `localhost:5432` (used by Hangfire)

### Configuration

Before running the application, you must provide your OpenWeatherMap API key.

1.  Obtain a free API key from [OpenWeatherMap](https://openweathermap.org/api).
2.  Open the `appsettings.json` file located in the `WeatherPulse.API` project.
3.  Replace the placeholder value for `OpenWeather:ApiKey`:

```json
{
  "OpenWeather": {
    "ApiKey": "YOUR_API_KEY_HERE"
  }
}
```

For development environments, it is recommended to use the Secret Manager tool to avoid checking sensitive keys into source control:

```bash
dotnet user-secrets set "OpenWeather:ApiKey" "YOUR_API_KEY_HERE"
```

### Running the Application

#### Via Visual Studio

1.  Open the `WeatherPulse.sln` solution.
2.  Set `WeatherPulse.API` as the startup project.
3.  Press `F5` to run with debugging.

#### Via .NET CLI

```bash
cd src/4.API/WeatherPulse.API
dotnet run
```

The API will be accessible at `https://localhost:5001` and `http://localhost:5000`. The Swagger UI is available at `/swagger`.

## API Documentation

### Get Outdoor Score

Fetches the aggregated weather data and calculates the outdoor score for a specified city.

```http
GET /api/weather/score?city={cityName}
```

**Example Request**

```http
GET /api/weather/score?city=London
```

**Example Response (Success - 200 OK)**

```json
{
  "city": "London",
  "tempC": 18.5,
  "aqi": 45,
  "uv": 2.3,
  "outdoorScore": 85,
  "advice": "Perfect for running!"
}
```

**Response Headers**  
If the result is served from the Redis cache, the response will contain the header `X-Cache: HIT`. Otherwise, it will contain `X-Cache: MISS`.

## Project Structure

```
WeatherPulse/
├── src/
│   ├── 1.Domain/
│   │   └── WeatherPulse.Domain/           # Entities & Enums
│   ├── 2.Application/
│   │   └── WeatherPulse.Application/      # DTOs & Service Interfaces
│   ├── 3.Infrastructure/
│   │   └── WeatherPulse.Infrastructure/   # Clients, Polly, Redis, Hangfire
│   └── 4.API/
│       └── WeatherPulse.API/              # Controllers, Middleware, Program.cs
├── tests/                                 # Unit & Integration Tests
├── docker-compose.yml                     # Local infrastructure orchestration
└── README.md
```

## License

This project is licensed under the MIT License.