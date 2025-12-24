using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuestionService.Data.Models;

namespace QuestionService.Data.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.HasData(
            // .NET Ecosystem
            new Tag
            {
                Id = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
                Name = "Aspire",
                Slug = "aspire",
                Description =
                    "A composable, opinionated stack for building distributed apps with .NET. Provides dashboard, diagnostics, and simplified service orchestration."
            },
            new Tag
            {
                Id = "b2c3d4e5-f6a7-8901-bcde-f12345678901",
                Name = ".NET",
                Slug = "dotnet",
                Description =
                    "A modern, cross-platform development platform for building cloud, web, mobile, desktop, and IoT apps using C# and F#."
            },
            new Tag
            {
                Id = "c3d4e5f6-a7b8-9012-cdef-123456789012",
                Name = "Entity Framework Core",
                Slug = "ef-core",
                Description =
                    "A modern object-database mapper (ORM) for .NET that supports LINQ, change tracking, and migrations for working with relational databases."
            },
            new Tag
            {
                Id = "d4e5f6a7-b8c9-0123-def0-234567890123",
                Name = "ASP.NET Core",
                Slug = "aspnet-core",
                Description =
                    "A cross-platform, high-performance framework for building modern, cloud-enabled, Internet-connected applications including web apps, APIs, and microservices."
            },
            new Tag
            {
                Id = "e5f6a7b8-c9d0-1234-ef01-345678901234",
                Name = "Blazor",
                Slug = "blazor",
                Description =
                    "A framework for building interactive web UIs using C# instead of JavaScript. Supports both server-side and WebAssembly client-side hosting models."
            },
            new Tag
            {
                Id = "f6a7b8c9-d0e1-2345-f012-456789012345",
                Name = ".NET MAUI",
                Slug = "maui",
                Description =
                    "A cross-platform framework for creating native mobile and desktop apps with C# and XAML. Successor to Xamarin.Forms with unified project structure."
            },
            new Tag
            {
                Id = "a7b8c9d0-e1f2-3456-0123-567890123456",
                Name = "Minimal APIs",
                Slug = "minimal-apis",
                Description =
                    "A simplified approach to building HTTP APIs in ASP.NET Core with minimal code and configuration. Ideal for microservices and small endpoints."
            },
            
            // Messaging & Communication
            new Tag
            {
                Id = "b8c9d0e1-f2a3-4567-1234-678901234567",
                Name = "Wolverine",
                Slug = "wolverine",
                Description =
                    "A high-performance messaging and command-handling framework for .NET with built-in support for Mediator, queues, retries, and durable messaging."
            },
            new Tag
            {
                Id = "c9d0e1f2-a3b4-5678-2345-789012345678",
                Name = "SignalR",
                Slug = "signalr",
                Description =
                    "A real-time communication library for ASP.NET that enables server-to-client messaging over WebSockets, long polling, and more."
            },
            new Tag
            {
                Id = "d0e1f2a3-b4c5-6789-3456-890123456789",
                Name = "MassTransit",
                Slug = "masstransit",
                Description =
                    "A distributed application framework for .NET that provides a consistent abstraction over message transports like RabbitMQ, Azure Service Bus, and Amazon SQS."
            },
            new Tag
            {
                Id = "e1f2a3b4-c5d6-7890-4567-901234567890",
                Name = "RabbitMQ",
                Slug = "rabbitmq",
                Description =
                    "An open-source message broker that implements AMQP protocol. Enables reliable messaging between distributed systems with routing, queuing, and pub/sub patterns."
            },
            new Tag
            {
                Id = "f2a3b4c5-d6e7-8901-5678-012345678901",
                Name = "gRPC",
                Slug = "grpc",
                Description =
                    "A high-performance, open-source RPC framework using Protocol Buffers. Enables efficient communication between microservices with strongly-typed contracts."
            },
            
            // Databases
            new Tag
            {
                Id = "a3b4c5d6-e7f8-9012-6789-123456789012",
                Name = "PostgreSQL",
                Slug = "postgresql",
                Description =
                    "A powerful, open-source object-relational database system known for reliability, feature richness, and standards compliance."
            },
            new Tag
            {
                Id = "b4c5d6e7-f8a9-0123-7890-234567890123",
                Name = "MongoDB",
                Slug = "mongodb",
                Description =
                    "A document-oriented NoSQL database designed for scalability and flexibility. Stores data in JSON-like documents with dynamic schemas."
            },
            new Tag
            {
                Id = "c5d6e7f8-a9b0-1234-8901-345678901234",
                Name = "Redis",
                Slug = "redis",
                Description =
                    "An in-memory data structure store used as database, cache, message broker, and streaming engine. Known for sub-millisecond response times."
            },
            new Tag
            {
                Id = "d6e7f8a9-b0c1-2345-9012-456789012345",
                Name = "SQL Server",
                Slug = "sql-server",
                Description =
                    "Microsoft's enterprise relational database management system with comprehensive features for transaction processing, analytics, and business intelligence."
            },
            new Tag
            {
                Id = "e7f8a9b0-c1d2-3456-0123-567890123456",
                Name = "Azure Cosmos DB",
                Slug = "cosmos-db",
                Description =
                    "A globally distributed, multi-model database service by Microsoft Azure. Offers turnkey global distribution, elastic scaling, and multiple consistency models."
            },
            
            // Security & Identity
            new Tag
            {
                Id = "f8a9b0c1-d2e3-4567-1234-678901234567",
                Name = "Keycloak",
                Slug = "keycloak",
                Description =
                    "An open-source identity and access management solution for modern applications and services. Integrates easily with OAuth2, OIDC, and SSO."
            },
            new Tag
            {
                Id = "a9b0c1d2-e3f4-5678-2345-789012345678",
                Name = "OAuth 2.0",
                Slug = "oauth2",
                Description =
                    "An authorization framework that enables applications to obtain limited access to user accounts. Industry standard for token-based authentication flows."
            },
            new Tag
            {
                Id = "b0c1d2e3-f4a5-6789-3456-890123456789",
                Name = "JWT",
                Slug = "jwt",
                Description =
                    "JSON Web Tokens are compact, URL-safe tokens for securely transmitting claims between parties. Commonly used for authentication and information exchange."
            },
            new Tag
            {
                Id = "c1d2e3f4-a5b6-7890-4567-901234567890",
                Name = "Duende IdentityServer",
                Slug = "identity-server",
                Description =
                    "An OpenID Connect and OAuth 2.0 framework for ASP.NET Core. Provides centralized authentication and authorization for modern applications."
            },
            
            // Frontend
            new Tag
            {
                Id = "d2e3f4a5-b6c7-8901-5678-012345678901",
                Name = "Next.js",
                Slug = "nextjs",
                Description =
                    "A React framework for building fast, full-stack web apps with server-side rendering, routing, and static site generation."
            },
            new Tag
            {
                Id = "e3f4a5b6-c7d8-9012-6789-123456789012",
                Name = "TypeScript",
                Slug = "typescript",
                Description =
                    "A statically typed superset of JavaScript that compiles to clean JavaScript. Helps build large-scale apps with tooling support."
            },
            new Tag
            {
                Id = "f4a5b6c7-d8e9-0123-7890-234567890123",
                Name = "React",
                Slug = "react",
                Description =
                    "A JavaScript library for building user interfaces with component-based architecture. Developed by Meta with a large ecosystem and community."
            },
            new Tag
            {
                Id = "a5b6c7d8-e9f0-1234-8901-345678901234",
                Name = "Tailwind CSS",
                Slug = "tailwindcss",
                Description =
                    "A utility-first CSS framework for rapidly building custom user interfaces. Provides low-level utility classes for flexible, responsive designs."
            },
            
            // Architecture & Patterns
            new Tag
            {
                Id = "b6c7d8e9-f0a1-2345-9012-456789012345",
                Name = "Microservices",
                Slug = "microservices",
                Description =
                    "An architectural style that structures an application as a collection of loosely coupled services that can be independently deployed and scaled."
            },
            new Tag
            {
                Id = "c7d8e9f0-a1b2-3456-0123-567890123456",
                Name = "CQRS",
                Slug = "cqrs",
                Description =
                    "Command Query Responsibility Segregation separates read and write operations into different models. Enables optimized scaling and complex domain modeling."
            },
            new Tag
            {
                Id = "d8e9f0a1-b2c3-4567-1234-678901234567",
                Name = "Event Sourcing",
                Slug = "event-sourcing",
                Description =
                    "A pattern where state changes are stored as a sequence of events. Provides complete audit trail and enables temporal queries and event replay."
            },
            new Tag
            {
                Id = "e9f0a1b2-c3d4-5678-2345-789012345678",
                Name = "Domain-Driven Design",
                Slug = "ddd",
                Description =
                    "An approach to software development that centers the design on the core domain and domain logic. Emphasizes ubiquitous language and bounded contexts."
            },
            new Tag
            {
                Id = "f0a1b2c3-d4e5-6789-3456-890123456789",
                Name = "Clean Architecture",
                Slug = "clean-architecture",
                Description =
                    "An architectural pattern that separates concerns into layers with dependencies pointing inward. Promotes testability, maintainability, and independence from frameworks."
            },
            
            // DevOps & Cloud
            new Tag
            {
                Id = "a1b2c3d4-e5f6-7890-4567-901234567890",
                Name = "Docker",
                Slug = "docker",
                Description =
                    "A platform for developing, shipping, and running applications in containers. Enables consistent environments across development, testing, and production."
            },
            new Tag
            {
                Id = "b2c3d4e5-f6a7-8901-5678-012345678901",
                Name = "Kubernetes",
                Slug = "kubernetes",
                Description =
                    "An open-source container orchestration platform for automating deployment, scaling, and management of containerized applications across clusters."
            },
            new Tag
            {
                Id = "c3d4e5f6-a7b8-9012-6789-123456789012",
                Name = "Microsoft Azure",
                Slug = "azure",
                Description =
                    "Microsoft's cloud computing platform offering IaaS, PaaS, and SaaS services. Provides deep integration with .NET and enterprise tooling."
            },
            new Tag
            {
                Id = "d4e5f6a7-b8c9-0123-7890-234567890123",
                Name = "GitHub Actions",
                Slug = "github-actions",
                Description =
                    "A CI/CD platform integrated into GitHub for automating build, test, and deployment workflows. Uses YAML-based workflow definitions."
            },
            new Tag
            {
                Id = "e5f6a7b8-c9d0-1234-8901-345678901234",
                Name = "Terraform",
                Slug = "terraform",
                Description =
                    "An infrastructure as code tool for building, changing, and versioning cloud infrastructure safely and efficiently using declarative configuration."
            },
            
            // Testing
            new Tag
            {
                Id = "f6a7b8c9-d0e1-2345-9012-456789012345",
                Name = "xUnit",
                Slug = "xunit",
                Description =
                    "A free, open-source unit testing framework for .NET. Known for its extensibility, parallel test execution, and modern design patterns."
            },
            new Tag
            {
                Id = "a7b8c9d0-e1f2-3456-0123-567890123457",
                Name = "Integration Testing",
                Slug = "integration-testing",
                Description =
                    "Testing that verifies different modules or services work together correctly. In ASP.NET Core, often uses WebApplicationFactory for in-memory testing."
            },
            new Tag
            {
                Id = "b8c9d0e1-f2a3-4567-1234-678901234568",
                Name = "Testcontainers",
                Slug = "testcontainers",
                Description =
                    "A library that provides lightweight, throwaway instances of databases, message brokers, and other services in Docker containers for integration testing."
            }
        );
    }
}
