# SpaceX Launch Data Service 🚀

A .NET 9 minimal API service for accessing SpaceX launch data with filtering, pagination, and automated synchronization.

## API Choice

**SpaceX API (v4)** - `https://api.spacexdata.com/v4/`
- Comprehensive launch data with detailed mission information
- Well-documented REST API with consistent data structure
- No authentication required for public launch data
- Active community and reliable uptime

## Functionality Built

### Core Features
- **Launch Data Retrieval**: Get paginated launches with advanced filtering (date range, success status, search terms)
- **Launch Details**: Retrieve specific launch information by ID
- **Data Synchronization**: Background CRON job updates SQLite database every 5 minutes
- **Manual Sync**: On-demand data refresh via API endpoint

### Technical Implementation
- **Async SQLite Database**: Persistent storage using Dapper ORM
- **Vertical Slice Architecture**: Clean, feature-based code organization
- **Type-Safe Error Handling**: OneOf pattern for explicit error management
- **Comprehensive Testing**: 27 unit and integration tests with mock data
- **API Documentation**: Swagger/OpenAPI with detailed examples

### Available Endpoints
- `GET /api/v1/launches` - Paginated launches with filtering/sorting
- `GET /api/v1/launches/{id}` - Specific launch details
- `POST /api/v1/launches/sync` - Manual data synchronization
- `GET /health` - Health check

## Improvements with More Time

### Performance & Scalability
- **Caching Layer**: Redis for frequently accessed data
- **Database Optimization**: Indexes and query optimization for large datasets
- **Rate Limiting**: Protect against API abuse
- **Pagination Improvements**: Cursor-based pagination for better performance

### Features
- **Data Enrichment**: Integrate additional SpaceX endpoints (rockets, payloads, crew)
- **Search Enhancement**: Full-text search with Elasticsearch
- **Historical Analytics**: Launch success trends and statistics

### Production Readiness
- **Authentication & Authorization**: JWT-based security
- **Monitoring**: Application insights and metrics collection
- **Configuration Management**: Azure Key Vault or similar
- **CI/CD Pipeline**: Automated testing, deployment, and database migrations