# Flux Project Guidelines

## Tech Stack
- **Framework:** .NET 10 (C#)
- **UI Framework:** Blazor (Razor Components)
- **ORM:** Entity Framework Core (EF Core)
- **Real-time Communication:** SignalR
- **Frontend Assets:** HTML, CSS, JavaScript (WebRTC, Voice/Video Recorders)

## Architecture
This project implements **Clean Architecture** combined with feature-based organization (Vertical Slices). Maintain the following boundaries:

1. **Domain (`/Flux/Domain`)**: 
   - The core of the application. Contains Entities, Value Objects, and Domain exceptions/results.
   - **Rule:** Zero external dependencies. It must not reference `Infrastructure`, `Features`, or any framework-specific libraries (like EF Core or ASP.NET Core).

2. **Features / Application (`/Flux/Features`)**:
   - Contains business use cases organized by feature (e.g., Auth, Channels, Messages).
   - **Rule:** References `Domain`. Does not reference `Infrastructure` directly. Should rely on interfaces defined in `Domain` or within the application layer.

3. **Infrastructure (`/Flux/Infrastructure`)**:
   - Contains external concerns: Database implementation (`FluxDbContext`), Services (Email), Identity (JWT, AuthStateProvider), External Clients, and SignalR Hubs.
   - **Rule:** References `Domain` and `Features`. Implement interfaces defined in inner layers.

4. **UI / Components (`/Flux/Components`)**:
   - Blazor Web UI (Pages, Layouts, UI primitives).
   - **Rule:** Depends on `Features` and `Infrastructure` (via Dependency Injection).

## Clean Code Rules

1. **SOLID Principles:** Strictly adhere to Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, and Dependency Inversion.
2. **Naming Conventions:**
   - Classes, Records, Methods, Properties: `PascalCase`
   - Local variables, method parameters: `camelCase`
   - Private fields: `_camelCase`
   - Interfaces: Prefix with `I` (e.g., `IRepository`)
3. **Result Pattern:** Use the `Result` pattern (`Domain/Common/Result.cs`) for handling flow control and validation. Avoid throwing exceptions for predictable business logic errors.
4. **Immutability:** Use `record` for DTOs/Requests. Use `init` properties and `readonly` fields wherever state mutation is not required.
5. **Dependency Injection:** Use Constructor Injection exclusively.
6. **Keep it Small:** Classes and methods should be small and do one thing well. Extract complex logic.
7. **Async / Await:** Use asynchronous programming (`async/await`) for all I/O bound operations. Pass `CancellationToken` where appropriate. Ensure async methods are suffixed with `Async` (unless framework conventions dictate otherwise).
8. **Self-Documenting Code:** Prioritize expressive variable and method names over comments. Use comments only to explain *why* (business reasoning), not *what*. Write XML documentation (`///`) for complex public interfaces.
