# SpaceX Launch Data Service 🚀

A comprehensive .NET 9 minimal API service for managing SpaceX launch data with background synchronization, health monitoring, and comprehensive testing.

## Project Structure

```
SpaceXLaunchDataService/
├── SpaceXLaunchDataService.Api/     # 🎯 Main API Project
│   ├── Program.cs                   # Application entry point
│   ├── Endpoints.cs                 # Central endpoint mapping
│   ├── GlobalUsings.cs             # Global using directives
│   ├── Common/                     # 🔧 Shared infrastructure
│   │   ├── Extensions/             # Service registration extensions
│   │   └── Services/               # Shared services and configuration
│   ├── Data/                       # 📊 Data access layer
│   │   ├── Types/                  # DTOs, domain models, and error types
│   │   ├── ILaunchRepository.cs    # Repository interface
│   │   └── LaunchRepository.cs     # Repository implementation
│   └── Features/                   # 🚀 Feature-driven organization
│       └── Launches/               # Launch management feature
│           ├── Endpoints/          # Individual endpoint classes
│           └── Services/           # Launch-specific business logic
└── SpaceXLaunchDataService.Tests/  # 🧪 Comprehensive test suite
    ├── IntegrationTests/           # Full API integration tests
    └── UnitTests/                  # Repository, service, and endpoint tests
```

## Features

### 🛠️ Core Architecture
- **Vertical Slice Architecture (VSA)**: Features organized by business capability
- **Minimal APIs**: Clean, focused endpoint definitions
- **Dependency Injection**: Built-in .NET DI container
- **Background Services**: Automated data synchronization

### 📊 Data Management
- **Repository Pattern**: Clean data access abstraction
- **SQLite + Dapper**: Lightweight, performant data layer
- **OneOf Pattern**: Type-safe error handling
- **DTO Mapping**: Clean separation between API and domain models

### 🔒 Error Handling & Resilience
- **OneOf Union Types**: Type-safe error propagation
- **Structured Error Responses**: Consistent API error format
- **Validation**: Input validation with clear error messages

### 🏥 Health Monitoring
- **Health Checks**: Built-in endpoint monitoring
- **Logging**: Comprehensive logging throughout the application
- **Background Service Health**: Monitor sync service status

### 📚 API Documentation
- **Swagger/OpenAPI**: Interactive API documentation
- **Response Examples**: Comprehensive examples for all endpoints
- **Error Documentation**: Detailed error response schemas

### 🧪 Testing Infrastructure
- **Unit Tests**: xUnit with FluentAssertions for comprehensive coverage
- **Integration Tests**: Full API endpoint testing with WebApplicationFactory
- **Mocking**: Moq for service layer testing
- **Type-safe Testing**: OneOf integration for error scenario testing

## Quick Start

1. **Clone and Build**
   ```bash
   git clone <repository-url>
   cd SpaceXLaunchDataService
   dotnet restore
   dotnet build
   ```

2. **Run Application**
   ```bash
   cd SpaceXLaunchDataService.Api
   dotnet run
   ```

3. **Access Endpoints**
   - API: `http://localhost:5000/api/v1/launches`
   - Swagger: `http://localhost:5000/swagger`
   - Health: `http://localhost:5000/health`

4. **Run Tests**
   ```bash
   dotnet test
   ```

## API Endpoints

### Launches
- `GET /api/v1/launches` - Get paginated launches
- `GET /api/v1/launches/{id}` - Get specific launch by ID
- `POST /api/v1/launches/sync` - Trigger manual data synchronization

### Health
- `GET /health` - Application health status

## Configuration

The application uses standard .NET configuration:
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- Environment variables - Production configuration

Key settings:
```json
{
  "SpaceXApi": {
    "BaseUrl": "https://api.spacexdata.com/v4/"
  }
}
```

## Development

### Project Architecture
The solution follows Vertical Slice Architecture (VSA) principles:
- Features are organized by business capability
- Each feature contains its own endpoints, services, and tests
- Shared infrastructure is kept in Common/
- Clean separation between API and data layers

### Adding New Features
1. Create feature folder in `Features/`
2. Add endpoints in `Features/{Feature}/Endpoints/`
3. Add services in `Features/{Feature}/Services/`
4. Register services in `ServiceExtensions.cs`
5. Map endpoints in `Program.cs`

### Testing Strategy
- Unit tests for individual components
- Integration tests for full API workflows
- Repository tests with in-memory database
- Service tests with mocked dependencies

## Architecture Decisions

### Why Vertical Slice Architecture?
- **Feature Cohesion**: Related code stays together
- **Reduced Coupling**: Features are largely independent
- **Easy Navigation**: Find all code for a feature in one place
- **Team Scalability**: Multiple teams can work on different features

### Why Minimal APIs?
- **Performance**: Reduced overhead compared to MVC
- **Simplicity**: Less ceremony, more focus on business logic
- **Modern .NET**: Latest patterns and practices
- **Testability**: Easy to test individual endpoints

### Why OneOf Pattern?
- **Type Safety**: Compile-time error handling
- **Explicit Error Handling**: No hidden exceptions
- **Railway-Oriented Programming**: Clear success/failure paths
- **Better API Contracts**: Clients know what to expect

## Contributing

1. Follow the established architecture patterns
2. Add comprehensive tests for new features
3. Update documentation for API changes
4. Use meaningful commit messages
5. Ensure all tests pass before submitting

## License

This project is licensed under the MIT License - see the LICENSE file for details.