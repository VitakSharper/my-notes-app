# Overflow

A Stack&nbsp;Overflow-style Q&A app built as a distributed system: .NET 10 services orchestrated by
[.NET Aspire](https://learn.microsoft.com/dotnet/aspire/) 13, event-driven messaging over RabbitMQ
with [Wolverine](https://wolverinefx.net/), full-text search in Typesense, identity in Keycloak, and
a Next.js 16 client called **Cairn**.

Built following Neil Cummings' Udemy course
_Build a complete distributed app using .NET Aspire_ (the course uses PostgreSQL; this repo runs on
SQL Server). Client authentication is the next chapter, so everything below is anonymous read/write
through Postman and read-only in the browser.

## Architecture

```
                    ┌──────────────┐
   browser ────────►│   webapp     │  Next.js 16 (Cairn) - :3000
                    └──────┬───────┘
                           │ server actions
                    ┌──────▼───────┐
                    │   gateway    │  YARP (Aspire AddYarp) - :8001
                    └──┬────────┬──┘
            /questions │        │ /search
            /tags      │        │
              ┌────────▼──┐  ┌──▼─────────┐
              │ question- │  │ search-svc │
              │    svc    │  │            │
              └──┬────┬───┘  └──┬─────┬───┘
                 │    │         │     │
        SQL Server    │    Typesense  │
        (questionDb)  │               │
                      └──► RabbitMQ ──┘
                        exchange "questions"
                        queue "questions.search"
```

| Project | Role |
| --- | --- |
| `Overflow.AppHost` | Aspire orchestration: containers, parameters, YARP routes, the webapp, and the Docker Compose environment |
| `QuestionService` | `/questions` and `/tags` controllers, Keycloak JWT bearer auth, publishes domain events |
| `QuestionService.Data` | `QuestionDbContext`, entities, repositories, EF Core migrations (applied at startup) |
| `SearchService` | `/search` and `/search/similar-titles` minimal APIs over Typesense, kept in sync by message handlers |
| `Contracts` | Event contracts shared by publisher and subscriber (`QuestionCreated`, `AnswerAccepted`, …) |
| `Common` | Shared Keycloak authentication and Wolverine/RabbitMQ wiring |
| `Overflow.ServiceDefaults` | OpenTelemetry, health checks, service discovery |
| `webapp` | Next.js App Router client: HeroUI, Tailwind v4, Zustand, date-fns, tiptap |
| `minio` (container) | S3-compatible storage for the images pasted into the editor, in place of the course's Cloudinary account. Console on http://localhost:9001 |

## Prerequisites

- .NET 10 SDK
- Node.js 20+ (for the client)
- Docker Desktop (SQL Server, Keycloak, RabbitMQ, Typesense all run as containers)
- The [Aspire CLI](https://learn.microsoft.com/dotnet/aspire/cli/install) for `aspire run` / `aspire publish`

The Typesense API key is an Aspire parameter and is not in the repo. Set it once:

```bash
dotnet user-secrets set "Parameters:typesense-api-key" "<your-key>" --project Overflow.AppHost
```

## Running in development

```bash
npm install --prefix webapp   # first time only
aspire run                    # or: dotnet run --project Overflow.AppHost
```

Aspire starts the containers, both services, the gateway and `next dev`, and injects `GATEWAY_URL`
into the client so nothing is hardcoded. Open the Aspire dashboard from the console output.

| Endpoint | URL |
| --- | --- |
| Client | http://localhost:3000 |
| Gateway | http://localhost:8001 |
| Keycloak | http://localhost:6001 |
| RabbitMQ management | http://localhost:15672 |
| Typesense | http://localhost:8108 |

To run the client on its own (`npm run dev` in `webapp`), create `webapp/.env.local` — it is
gitignored:

```
API_URL=http://localhost:8001
AUTH_URL=http://localhost:3000
AUTH_SECRET=<openssl rand -base64 32>
AUTH_KEYCLOAK_ID=nextjs
AUTH_KEYCLOAK_SECRET=<the nextjs client secret from Keycloak>
AUTH_KEYCLOAK_ISSUER=http://localhost:6001/realms/overflow
```

Under Docker Compose the issuer is `http://id.overflow.local/realms/overflow` instead, since
Keycloak publishes no host port there. Note that `npx auth secret` now resolves to an unrelated
package on npm and writes a `BETTER_AUTH_SECRET`; generate `AUTH_SECRET` yourself.

## Running on Docker Compose

`aspire publish` only renders the compose file; it does **not** build the service images, and there
are no Dockerfiles (the .NET SDK builds the containers).

```bash
aspire publish -o Overflow.AppHost/infra --non-interactive

dotnet publish QuestionService/QuestionService.csproj -c Release /t:PublishContainer \
  -p:ContainerRepository=question-svc -p:ContainerImageTag=latest
dotnet publish SearchService/SearchService.csproj -c Release /t:PublishContainer \
  -p:ContainerRepository=search-svc -p:ContainerImageTag=latest

cd Overflow.AppHost/infra && docker compose -p overflow up -d
```

`ContainerRepository` has to be forced, otherwise the default image name does not match the compose
file. Run compose from `infra/` so it picks up the generated `.env`.

`aspire publish` writes `infra/.env` with the container passwords in clear text, so that file is
gitignored. It is regenerated by the publish step, except for parameters declared with
`AddParameter`: those are written as empty placeholders and have to be filled in from the AppHost
user secrets (`dotnet user-secrets list --project Overflow.AppHost`). Today that means `MINIO_USER`
and `MINIO_PASSWORD`.

In this mode an `nginx-proxy` container routes by `Host` header instead of publishing ports, so add
to your hosts file:

```
127.0.0.1 api.overflow.local
127.0.0.1 id.overflow.local
```

The gateway is then at http://api.overflow.local, Keycloak (including the admin console) at
http://id.overflow.local, and the Aspire dashboard on http://localhost:8080. The client app is not
part of the generated compose file yet — it still needs `PublishAsDockerFile()`.

## Identity

The `overflow` realm is imported automatically from `Overflow.AppHost/infra/realms` the first time
the `keycloak-data` volume is created: public client `postman` and users `admin`, `bob`, `dave`.
Passwords live in `Overflow.AppHost/infra/.env`. Postman collections live outside this repo, in
`OverflowAssets/postman`.

Two things are **not** in that import and live only in the `keycloak-data` volume, so deleting it
means recreating them:

- the confidential client `nextjs` used by the web app (root URL `http://localhost:3000`, redirect
  `/*`, an `oidc-audience-mapper` adding the `overflow` audience, and `Full scope allowed` off).
  Its secret goes into `webapp/.env.local` as `AUTH_KEYCLOAK_SECRET` - it is deliberately kept out
  of the realm file, which is committed;
- the permanent master-realm admin `kc-admin`, which replaced the temporary bootstrap admin.

User registration is enabled on the realm, which is what makes the Register button work.

## Client app (Cairn)

| Route | Content |
| --- | --- |
| `/questions` | Question list, `?tag=<slug>` filters it |
| `/questions/[id]` | Question detail with answers |
| `/tags` | Tag cards, each links to the filtered list |
| `/session` | Buttons that trigger every API error code, to exercise error handling |

Conventions worth knowing before editing `webapp`:

- `src/lib/fetch-client.ts` is the only place that calls `fetch`. It returns `{ data, error }`
  rather than throwing, because a server action cannot throw across the server/client boundary. A
  404 routes to the not-found page and a 500 to the error boundary; everything else comes back as an
  error the caller turns into a toast with `handleError`.
- Server pages throw so `app/error.tsx` catches; client components call a server action inside
  `useTransition` and show a toast.
- Tags are fetched with `force-cache` and a one-hour revalidate, then held in a Zustand store
  (`src/lib/hooks/use-tag-store.ts`) so any component can resolve a slug to its description.
- Dates are formatted with `date-fns` through `timeAgo` / `fuzzyTimeAgo` in `src/lib/util.ts`.
- Images pasted into the editor go through `POST /api/images`, which checks the session and stores
  the file in MinIO (`src/lib/storage.ts`); the response carries the public URL and the key used to
  delete it later. The bucket is created on first upload and opened for anonymous reads only.

## Known issues

- Answer counts on the tag cards are hardcoded to 42 — the API does not expose the count per tag yet.
- `DELETE /questions/{id}` does not check ownership: any authenticated user can delete any question.
- The container passwords that were committed in `infra/.env` before it was gitignored are still in
  the git history. They only protect local containers, and rotating the Keycloak one would mean
  recreating the volume and losing the realm setup, so they were left alone.
