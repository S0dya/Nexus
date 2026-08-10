# Nexus

A scalable, production-ready game backend API built with ASP.NET Core. Designed as a generic backend solution that can be adapted for different game types, this project demonstrates real-world backend architecture patterns and best practices.

## 🎯 Project Overview

Nexus was built with two primary goals:
1. **Create a flexible game backend** that handles authentication, player data, leaderboards, inventory, and monetization
2. **Learn production-grade development** by implementing systems in a real-world context with proper architecture, testing, and deployment considerations

## ✨ Features

### 🔐 Authentication System
- **JWT-based authentication** with access and refresh tokens
- **Multi-device support** - users can authenticate from multiple devices simultaneously
- **Device management** with configurable expiry (default 180 days)
- **Token revocation** for secure logout
- **Password validation** with BCrypt hashing
- **Anonymous-to-registered user flow** support

### 👤 Player Profiles
- Customizable player profiles with bio, display name
- Avatar/icon system with configurable range
- Last online tracking with automatic updates
- Profile isolation per user

### 💾 Cloud Save System
- Simple string-based cloud storage
- Automatic save creation on account registration
- Per-user data isolation
- Easy integration with game clients

### 🏆 Leaderboard System
- **Global leaderboard** with configurable entry limits
- **Score submission** with validation
- **Personal best tracking** per player
- **Rank calculation** with efficient queries
- **Pagination support** for large datasets
- **Tie-breaking** using LastUpdated timestamp
- **Season-ready architecture** with SeasonId field for future seasonal leaderboards
- **Redis caching** for improved performance (30-second cache)

### 🎒 Inventory System
- **Multi-currency support** (Coins, Gems)
- **Item ownership tracking** with dedicated item table
- **Currency operations** - grant, spend, and balance checks
- **Item operations** - grant, remove, and ownership verification
- **Default starting currency** configuration
- **Business logic separation** in dedicated services
- **Database consistency** through proper transactions

### 🛒 Shop System
- **Dynamic shop offers** with start/end date validation
- **Offer enable/disable functionality**
- **Purchase orchestration** - shop coordinates with inventory
- **Transaction boundaries** for data consistency
- **Service-to-service communication** patterns
- **DTO mapping** for clean API responses

### 📊 Inventory Audit
- **Complete transaction history** for all inventory changes
- **Currency transaction records** with amounts and balances
- **Item transaction records** for grants/removals
- **Transaction reason enum** for categorization
- **ReferenceId support** for linking to external systems
- **Production debugging** capabilities through audit trails

### 🎮 Game Events
- **Event ingestion system** for tracking player actions
- **Batch processing** with configurable batch size (default: 100)
- **Background worker** for async event processing
- **Shop purchase tracking** through event payloads
- **Configurable processing intervals** (default: 60 seconds)

### 📈 Analytics
- **Player analytics tracking** per user
- **Session data** with duration and metadata
- **Configurable session parameters** (min/max duration, amounts)
- **Run history** with configurable limits

## 🛠 Tech Stack

### Core Framework
- **ASP.NET Core 10.0** - Web framework
- **Entity Framework Core 10.0** - ORM
- **PostgreSQL** - Primary database (via Npgsql)
- **Redis** - Caching layer (via StackExchange.Redis)

### Authentication & Security
- **JWT Bearer Authentication** - Token-based auth
- **BCrypt.Net-Next** - Password hashing
- **Serilog** - Structured logging

### API & Documentation
- **Swagger/OpenAPI** - API documentation
- **Bruno** - API testing (alternative to Swagger)

### Monitoring & Health
- **ASP.NET Core Health Checks** - Health monitoring
- **EF Core Health Checks** - Database health
- **Redis Health Checks** - Cache health

### Testing
- **xUnit** - Testing framework
- **Integration tests** with WebApplicationFactory

## 🏗 Architecture

### Project Structure
```
Nexus/
├── Features/              # Feature-based organization
│   ├── Auth/             # Authentication & device management
│   ├── Profile/          # Player profiles
│   ├── CloudSave/        # Cloud save system
│   ├── Leaderboard/      # Leaderboard with caching
│   ├── Inventory/        # Inventory & currency management
│   ├── Shop/             # Shop offers & purchases
│   ├── GameEvent/        # Event processing
│   └── Analytics/        # Player analytics
├── Infrastructure/       # Cross-cutting concerns
│   ├── DependencyInjection/  # DI configuration
│   ├── Exceptions/       # Custom exceptions
│   └── Security/         # Security utilities
├── Middlewares/          # ASP.NET Core middleware
│   ├── GlobalExceptionMiddleware.cs
│   └── LastOnlineMiddleware.cs
├── Database/            # EF Core context
│   └── AppDbContext.cs
├── Migrations/          # Database migrations
├── Options/             # Configuration options
└── Nexus.Tests/         # Integration tests
```

### Design Patterns
- **Feature-based architecture** - Each feature is self-contained
- **Repository pattern** via EF Core DbContext
- **Service layer pattern** - Business logic in dedicated services
- **DTO pattern** - Clean API contracts
- **Factory pattern** - Entity creation (DeviceFactory, CloudSaveFactory)
- **Middleware pipeline** - Cross-cutting concerns
- **Dependency Injection** - Loose coupling and testability

### Key Architectural Decisions
- **Items as separate table** - Better query performance vs JSON storage
- **Currencies in inventory** - Separated from cloud save for consistency
- **Audit logging** - Every inventory change leaves evidence
- **Service boundaries** - Shop orchestrates, Inventory owns logic
- **Season support** - Built into leaderboard from the start
- **Redis caching** - Leaderboard caching for performance

## 📚 What I Learned

### Backend Architecture
- **Service layer separation** - Business logic belongs in services, not controllers
- **Transaction boundaries** - Managing data consistency across operations
- **Service-to-service communication** - Clean interaction between features
- **Reusable business services** - DRY principle in action

### Database Design
- **Entity relationships** - One-to-one, one-to-many patterns
- **Indexing strategies** - Efficient query performance
- **DTO projection** - Select only needed data
- **Migration management** - EF Core migrations workflow
- **Why items deserve their own table** - Performance vs JSON trade-offs

### Authentication & Security
- **JWT token lifecycle** - Generation, validation, revocation
- **Multi-device authentication** - Device tracking and management
- **Password hashing** - BCrypt for security
- **Token refresh patterns** - Balancing security and UX

### Performance Optimization
- **Redis caching** - Reducing database load
- **Pagination** - Handling large datasets efficiently
- **Efficient queries** - Proper LINQ usage and indexing
- **Background processing** - Async event processing

### Production Considerations
- **Audit logging** - Traceability and debugging
- **Health checks** - Monitoring system health
- **Structured logging** - Serilog for production logs
- **Exception handling** - Global middleware for consistency
- **Configuration management** - appsettings patterns

### Testing
- **Integration testing** - WebApplicationFactory usage
- **API testing** - Bruno for endpoint testing
- **Test isolation** - Clean test setup/teardown

## 🚀 Getting Started

### Prerequisites
- .NET 10.0 SDK
- PostgreSQL 14+
- Redis 7+
- (Optional) Bruno for API testing

### Configuration

1. **Clone the repository**
```bash
git clone <repository-url>
cd Nexus
```

2. **Configure appsettings.json**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=nexus_db;Username=postgres;Password=your_password"
  },
  "Jwt": {
    "Secret": "your-secret-key",
    "Issuer": "nexus",
    "Audience": "GameClient",
    "ExpirationMinutes": 60
  },
  "Redis": {
    "ConnectionString": "localhost:6379",
    "DatabaseId": 0,
    "LeaderboardCacheSeconds": 30
  }
}
```

3. **Run database migrations**
```bash
dotnet ef database update
```

4. **Start Redis**
```bash
redis-server
```

5. **Run the application**
```bash
dotnet run
```

The API will be available at `http://localhost:10001` (or configured PORT).

### Running Tests
```bash
dotnet test
```

## 📡 API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login with credentials
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - Revoke tokens
- `GET /api/auth/me` - Get current user info

### Profile
- `GET /api/profile` - Get player profile
- `PUT /api/profile` - Update player profile

### Cloud Save
- `GET /api/cloudsave` - Load saved data
- `POST /api/cloudsave` - Save game data

### Leaderboard
- `GET /api/leaderboard` - Get global leaderboard (paginated)
- `POST /api/leaderboard/score` - Submit score
- `GET /api/leaderboard/rank/{userId}` - Get player rank

### Inventory
- `GET /api/inventory` - Get player inventory
- `POST /api/inventory/currency/grant` - Grant currency
- `POST /api/inventory/currency/spend` - Spend currency
- `POST /api/inventory/item/grant` - Grant item
- `POST /api/inventory/item/remove` - Remove item
- `POST /api/inventory/item/has` - Check item ownership

### Shop
- `GET /api/shop/offers` - Get all available offers
- `GET /api/shop/offers/{id}` - Get specific offer
- `POST /api/shop/purchase` - Purchase offer

### Health
- `GET /health` - Health check endpoint

## 🔮 Future Enhancements

- [ ] Complete seasonal leaderboard implementation
- [ ] Add more analytics events and metrics
- [ ] Implement push notifications
- [ ] Add guild/clan system
- [ ] Real-time multiplayer support
- [ ] Advanced anti-cheat measures
- [ ] CDN integration for assets
- [ ] A/B testing framework

## 📝 License

This project is for portfolio and educational purposes.

## 👤 Author

Built as a portfolio project to demonstrate production-ready backend development skills with ASP.NET Core, focusing on real-world game backend requirements and best practices.
