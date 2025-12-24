using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuestionService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Questions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    AskerId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    AskerDisplayName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    TagSlugs = table.Column<List<string>>(type: "text[]", nullable: false),
                    HasAcceptedAnswer = table.Column<bool>(type: "boolean", nullable: false),
                    Votes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Description", "Name", "Slug" },
                values: new object[,]
                {
                    { "a1b2c3d4-e5f6-7890-4567-901234567890", "A platform for developing, shipping, and running applications in containers. Enables consistent environments across development, testing, and production.", "Docker", "docker" },
                    { "a1b2c3d4-e5f6-7890-abcd-ef1234567890", "A composable, opinionated stack for building distributed apps with .NET. Provides dashboard, diagnostics, and simplified service orchestration.", "Aspire", "aspire" },
                    { "a3b4c5d6-e7f8-9012-6789-123456789012", "A powerful, open-source object-relational database system known for reliability, feature richness, and standards compliance.", "PostgreSQL", "postgresql" },
                    { "a5b6c7d8-e9f0-1234-8901-345678901234", "A utility-first CSS framework for rapidly building custom user interfaces. Provides low-level utility classes for flexible, responsive designs.", "Tailwind CSS", "tailwindcss" },
                    { "a7b8c9d0-e1f2-3456-0123-567890123456", "A simplified approach to building HTTP APIs in ASP.NET Core with minimal code and configuration. Ideal for microservices and small endpoints.", "Minimal APIs", "minimal-apis" },
                    { "a7b8c9d0-e1f2-3456-0123-567890123457", "Testing that verifies different modules or services work together correctly. In ASP.NET Core, often uses WebApplicationFactory for in-memory testing.", "Integration Testing", "integration-testing" },
                    { "a9b0c1d2-e3f4-5678-2345-789012345678", "An authorization framework that enables applications to obtain limited access to user accounts. Industry standard for token-based authentication flows.", "OAuth 2.0", "oauth2" },
                    { "b0c1d2e3-f4a5-6789-3456-890123456789", "JSON Web Tokens are compact, URL-safe tokens for securely transmitting claims between parties. Commonly used for authentication and information exchange.", "JWT", "jwt" },
                    { "b2c3d4e5-f6a7-8901-5678-012345678901", "An open-source container orchestration platform for automating deployment, scaling, and management of containerized applications across clusters.", "Kubernetes", "kubernetes" },
                    { "b2c3d4e5-f6a7-8901-bcde-f12345678901", "A modern, cross-platform development platform for building cloud, web, mobile, desktop, and IoT apps using C# and F#.", ".NET", "dotnet" },
                    { "b4c5d6e7-f8a9-0123-7890-234567890123", "A document-oriented NoSQL database designed for scalability and flexibility. Stores data in JSON-like documents with dynamic schemas.", "MongoDB", "mongodb" },
                    { "b6c7d8e9-f0a1-2345-9012-456789012345", "An architectural style that structures an application as a collection of loosely coupled services that can be independently deployed and scaled.", "Microservices", "microservices" },
                    { "b8c9d0e1-f2a3-4567-1234-678901234567", "A high-performance messaging and command-handling framework for .NET with built-in support for Mediator, queues, retries, and durable messaging.", "Wolverine", "wolverine" },
                    { "b8c9d0e1-f2a3-4567-1234-678901234568", "A library that provides lightweight, throwaway instances of databases, message brokers, and other services in Docker containers for integration testing.", "Testcontainers", "testcontainers" },
                    { "c1d2e3f4-a5b6-7890-4567-901234567890", "An OpenID Connect and OAuth 2.0 framework for ASP.NET Core. Provides centralized authentication and authorization for modern applications.", "Duende IdentityServer", "identity-server" },
                    { "c3d4e5f6-a7b8-9012-6789-123456789012", "Microsoft's cloud computing platform offering IaaS, PaaS, and SaaS services. Provides deep integration with .NET and enterprise tooling.", "Microsoft Azure", "azure" },
                    { "c3d4e5f6-a7b8-9012-cdef-123456789012", "A modern object-database mapper (ORM) for .NET that supports LINQ, change tracking, and migrations for working with relational databases.", "Entity Framework Core", "ef-core" },
                    { "c5d6e7f8-a9b0-1234-8901-345678901234", "An in-memory data structure store used as database, cache, message broker, and streaming engine. Known for sub-millisecond response times.", "Redis", "redis" },
                    { "c7d8e9f0-a1b2-3456-0123-567890123456", "Command Query Responsibility Segregation separates read and write operations into different models. Enables optimized scaling and complex domain modeling.", "CQRS", "cqrs" },
                    { "c9d0e1f2-a3b4-5678-2345-789012345678", "A real-time communication library for ASP.NET that enables server-to-client messaging over WebSockets, long polling, and more.", "SignalR", "signalr" },
                    { "d0e1f2a3-b4c5-6789-3456-890123456789", "A distributed application framework for .NET that provides a consistent abstraction over message transports like RabbitMQ, Azure Service Bus, and Amazon SQS.", "MassTransit", "masstransit" },
                    { "d2e3f4a5-b6c7-8901-5678-012345678901", "A React framework for building fast, full-stack web apps with server-side rendering, routing, and static site generation.", "Next.js", "nextjs" },
                    { "d4e5f6a7-b8c9-0123-7890-234567890123", "A CI/CD platform integrated into GitHub for automating build, test, and deployment workflows. Uses YAML-based workflow definitions.", "GitHub Actions", "github-actions" },
                    { "d4e5f6a7-b8c9-0123-def0-234567890123", "A cross-platform, high-performance framework for building modern, cloud-enabled, Internet-connected applications including web apps, APIs, and microservices.", "ASP.NET Core", "aspnet-core" },
                    { "d6e7f8a9-b0c1-2345-9012-456789012345", "Microsoft's enterprise relational database management system with comprehensive features for transaction processing, analytics, and business intelligence.", "SQL Server", "sql-server" },
                    { "d8e9f0a1-b2c3-4567-1234-678901234567", "A pattern where state changes are stored as a sequence of events. Provides complete audit trail and enables temporal queries and event replay.", "Event Sourcing", "event-sourcing" },
                    { "e1f2a3b4-c5d6-7890-4567-901234567890", "An open-source message broker that implements AMQP protocol. Enables reliable messaging between distributed systems with routing, queuing, and pub/sub patterns.", "RabbitMQ", "rabbitmq" },
                    { "e3f4a5b6-c7d8-9012-6789-123456789012", "A statically typed superset of JavaScript that compiles to clean JavaScript. Helps build large-scale apps with tooling support.", "TypeScript", "typescript" },
                    { "e5f6a7b8-c9d0-1234-8901-345678901234", "An infrastructure as code tool for building, changing, and versioning cloud infrastructure safely and efficiently using declarative configuration.", "Terraform", "terraform" },
                    { "e5f6a7b8-c9d0-1234-ef01-345678901234", "A framework for building interactive web UIs using C# instead of JavaScript. Supports both server-side and WebAssembly client-side hosting models.", "Blazor", "blazor" },
                    { "e7f8a9b0-c1d2-3456-0123-567890123456", "A globally distributed, multi-model database service by Microsoft Azure. Offers turnkey global distribution, elastic scaling, and multiple consistency models.", "Azure Cosmos DB", "cosmos-db" },
                    { "e9f0a1b2-c3d4-5678-2345-789012345678", "An approach to software development that centers the design on the core domain and domain logic. Emphasizes ubiquitous language and bounded contexts.", "Domain-Driven Design", "ddd" },
                    { "f0a1b2c3-d4e5-6789-3456-890123456789", "An architectural pattern that separates concerns into layers with dependencies pointing inward. Promotes testability, maintainability, and independence from frameworks.", "Clean Architecture", "clean-architecture" },
                    { "f2a3b4c5-d6e7-8901-5678-012345678901", "A high-performance, open-source RPC framework using Protocol Buffers. Enables efficient communication between microservices with strongly-typed contracts.", "gRPC", "grpc" },
                    { "f4a5b6c7-d8e9-0123-7890-234567890123", "A JavaScript library for building user interfaces with component-based architecture. Developed by Meta with a large ecosystem and community.", "React", "react" },
                    { "f6a7b8c9-d0e1-2345-9012-456789012345", "A free, open-source unit testing framework for .NET. Known for its extensibility, parallel test execution, and modern design patterns.", "xUnit", "xunit" },
                    { "f6a7b8c9-d0e1-2345-f012-456789012345", "A cross-platform framework for creating native mobile and desktop apps with C# and XAML. Successor to Xamarin.Forms with unified project structure.", ".NET MAUI", "maui" },
                    { "f8a9b0c1-d2e3-4567-1234-678901234567", "An open-source identity and access management solution for modern applications and services. Integrates easily with OAuth2, OIDC, and SSO.", "Keycloak", "keycloak" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Questions");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
