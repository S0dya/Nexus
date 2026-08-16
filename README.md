# Nexus

Nexus is a game backend built with ASP.NET Core and PostgreSQL. I built it for my own Unity projects and targeted it to contain what a real game client needs: login, player data, cloud saves, progression, inventory, a shop, leaderboards, analytics, and event handling.

## What's in it

- **Auth** - JWT access and refresh tokens, logout/revocation, anonymous accounts, and linking a guest account to a real one once the player signs up.
- **Devices** - each device gets its own refresh token and expiration, so a player logged in on two phones has two independent sessions the server can tell apart and revoke separately.
- **Player profiles** - display name, icon, bio, country, level, XP, last time seen online.
- **Cloud saves** - versioned, with optimistic concurrency so an old client can't silently stomp on a newer save.
- **Leaderboards** - score submission, personal bests, ranking, pagination, deterministic tie-breaking, Redis caching on top.
- **Inventory** - currencies and items live as real database entities, not as a blob of JSON tucked into the save file.
- **Shop** - time-limited offers, purchases run through the inventory system inside a single DB transaction.
- **Inventory audit** - every currency and item change gets a record with a reason and a reference ID, so you can actually trace where something came from.
- **Game events** - events get written first and processed later, asynchronously, in batches.
- **Analytics** - built around a generic event shape, so a new event type doesn't need a new table or a migration.
- **Health and observability** - health checks, structured logging with Serilog, one place that handles exceptions.
- **Rate limiting** - separate limits for auth, reads and writes.
- **Integration tests** - real API-level tests via `WebApplicationFactory`.
- **Docker** - API, PostgreSQL and Redis all containerized for a reproducible local setup.

## How it's structured

Feature-based folders:

```text
Features/
├── Auth/
├── Profile/
├── CloudSave/
├── Leaderboard/
├── Inventory/
├── Shop/
├── GameEvent/
├── Analytics/
└── Registration/

Infrastructure/
├── DependencyInjection/
├── Exceptions/
└── Security/

Database/
Middlewares/
Options/
Tests/
```

Controllers stay thin on purpose - they just handle the HTTP side and hand everything off to a service.

```text
HTTP Request
    ↓
Controller
    ↓
Service
    ↓
EF Core / Redis
    ↓
PostgreSQL
```

Features talk to each other through service interfaces, not by reaching into each other's data directly. A shop purchase, for example, doesn't reimplement inventory logic itself:

```text
Shop
 ├── validates offer
 ├── starts transaction
 ├── InventoryService.SpendCurrency()
 ├── InventoryService.GrantItem()
 └── writes audit records
```

The feature that owns a rule is the only one allowed to change it.

## A few decisions worth explaining

**Cloud save vs inventory.** Cloud save is opaque, whatever blob the client sends up. Inventory is not - currencies and items are real relational data, because the server needs to validate and change them on its own, and because purchases and audit logging need to be transactional. Mixing the two would mean either trusting the client's save blob for economy data, or splitting logic across two different storage models for no reason.

**Optimistic concurrency on saves.** Every save carries a version number. The client sends back the version it last received, and the server only accepts the write if that still matches what's stored.

```text
Client v5
    ↓
Server v5 → accept → Server v6

Client v5
    ↓
Server v6 → reject → conflict
```


**Where transactions start and end.** A purchase touches three things - spend currency, grant item, write the audit record - and all three live in one transaction. If any step fails, the whole thing rolls back. The shop owns the transaction because the purchase is its business operation; inventory just owns the actual mutations.

**Why there's an audit trail at all.** Inventory changes aren't throwaway state. Every purchase or currency/item change leaves a record with a reason, an amount, and a reference ID, so questions like "why does this player have this much gold" or "which offer granted this item" actually have an answer. 

**Leaderboards.** Queries are paginated and ordered at the database level - nothing gets pulled fully into memory. Redis caches results for a short time, with some jitter on the expiry so a wall of cache entries doesn't all expire at once.

**Analytics is deliberately generic.** Events are stored separately from whatever feature generated them, using a generic shape - event type, user, timestamp, and a serialized JSON payload - that adding a new event type doesn't mean a new table or migration. Events get written immediately and processed later by a background worker in batches, so a request never has to wait on analytics work.

```csharp
[Index(nameof(UserId), IsUnique = true)]
public class PlayerAnalyticsEntity
{
    public Guid UserId {get; set; }
    public int Purchases {get; set; }
    public int CoinsSpent {get; set; }
    public int ItemsBought {get; set; }
}
```

Event processing with pattern matching:
```csharp
switch (gameEvent.Type)
{
    case GameEventType.ShopPurchase:
        var payload = JsonSerializer.Deserialize<ShopPurchasePayload>(gameEvent.Payload);
        analytics.Purchases++;
        analytics.CoinsSpent += payload.CurrencySpent;
        analytics.ItemsBought += payload.ItemAmount;
        break;
    default: 
        throw new NotImplementedException($"Analytics doesn't support {gameEvent.Type}");
}
```

## Auth flow

Both real accounts and guest accounts are supported:

```text
Anonymous
    ↓
Guest account
    ↓
Play
    ↓
Link account
    ↓
Registered account
```

Refresh tokens belong to a device, not just a user, so a player can be logged in on multiple devices with independent sessions, and any one of them can be revoked without touching the others. Passwords are hashed with BCrypt, and secrets like JWT keys and DB credentials come from config, never from the repo.

## The less glamorous, more important stuff

Things that are easy to skip in a hobby API but matter once it's a real service:

* Global exception middleware, so errors come back consistent.

```csharp
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ApiException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            await HandleExceptionAsync(context, ex.Message);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            await HandleExceptionAsync(context, "Internal server error");
        }
    }
}
```

* Serilog instead of `Console.WriteLine`.
* Health checks for dependencies.
* Rate limiting, split by auth/read/write.
* Optimistic concurrency on saves.
* Transactions for anything multi-step.
* Audit history on player-state changes.
* Integration tests that hit real HTTP behavior.

## Testing

Integration tests via `WebApplicationFactory`, aimed at the whole request pipeline rather than isolated methods:

```text
HTTP request
    ↓
Middleware
    ↓
Authentication
    ↓
Controller
    ↓
Service
    ↓
Database / Redis
```

I also used Bruno day-to-day for manually poking at endpoints while building.

## Stack

C#, ASP.NET Core 10, EF Core 10, PostgreSQL, Redis / StackExchange.Redis, JWT bearer auth, BCrypt, Serilog, xUnit, ASP.NET Core integration testing, Docker / Docker Compose, Bruno, OpenAPI / Swagger.

## What this project actually taught me

The point was never "learn technology X." It was figuring out how these pieces fit together when you're building around what a real game needs, not around a tutorial. That meant working through: where a service boundary should sit and who owns a given rule, relational modeling and migrations, keeping things consistent across multi-step operations, session and device management, caching and when to invalidate it, background processing for anything that shouldn't block a request, validation and rate limiting and centralized error handling, integration testing actual API flows, config/secrets.

Nexus isn't finished in the sense of "done" - it's meant to keep growing as I plug it into future Unity projects. Right now it's a solid general foundation, not a checklist of every backend feature that could exist.