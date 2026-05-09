# AIFeed

> **AI news broker** — aggregates content from Hacker News, dev.to, arXiv, GitHub Trending and Product Hunt into a single, queryable feed.

[![Built with Arkn](https://img.shields.io/badge/Built%20with-Arkn-7c6af7?style=flat-square)](https://github.com/fernando-terra/arkn)
[![.NET 10](https://img.shields.io/badge/.NET-10-512bd4?style=flat-square)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

A canonical demo of the [Arkn framework](https://github.com/fernando-terra/arkn) — showing `Result<T>` error handling, `IArknJob` background scheduling, `ArknHttpClient` adapters, and vertical-slice architecture in a real-world API.

---

## Features

- **5 AI news sources** — HackerNews (Algolia), dev.to, arXiv (Atom), GitHub Search, Product Hunt (GraphQL)
- **Automatic refresh** — Arkn.Jobs cron scheduler pulls new items every 30 minutes
- **Daily & weekly digests** — top items grouped by source
- **Full-text search** — keyword + source + date range filtering
- **Rate limiting** — ASP.NET Core native (20 req/s global, 100 req/min per IP)
- **Zero dropped sources** — one failing source never blocks the others (`Result<T>`)
- **SQLite persistence** — lightweight, zero-config, EF Core migrations

---

## Architecture

Vertical Slice — each feature owns its endpoint, query logic and response shape:

```
AIFeed.Api/
├── Features/
│   ├── Sources/          → GET /sources, GET /sources/{id}/health
│   ├── Feed/             → GET /feed, POST /feed/refresh
│   ├── Digest/           → GET /digest/daily, GET /digest/weekly
│   └── Search/           → GET /search
├── Infrastructure/
│   ├── Persistence/      → AppDbContext, FeedItem, Migrations
│   ├── Sources/          → IFeedSource + 5 adapters
│   └── Jobs/             → RefreshFeedJob, CleanupJob
└── Program.cs            → DI, rate limiting, EF migrations
```

Each `IFeedSource` adapter:
1. Calls its external API using a typed `HttpClient`
2. Returns `Result<IReadOnlyList<FeedItem>>`
3. Failure = logged + skipped, never propagated up

---

## Quick start

```bash
git clone https://github.com/fernando-terra/aifeed
cd aifeed/AIFeed.Api
dotnet run
```

The API starts on `http://localhost:5000`. SQLite database is created automatically on first run.

### Optional env vars

| Variable | Purpose |
|---|---|
| `GITHUB_TOKEN` | GitHub PAT — raises rate limit from 60 to 5000 req/h |
| `PRODUCTHUNT_TOKEN` | Product Hunt Developer Token — enables PH source |

---

## API Endpoints

| Method | Path | Description |
|---|---|---|
| `GET` | `/health` | Health check |
| `GET` | `/sources` | List all registered sources |
| `GET` | `/sources/{id}/health` | Probe a single source |
| `GET` | `/feed` | Paginated feed (`?page=&size=&source=`) |
| `POST` | `/feed/refresh` | Trigger immediate refresh |
| `GET` | `/digest/daily` | Top items from last 24h by source |
| `GET` | `/digest/weekly` | Top items from last 7 days by source |
| `GET` | `/search` | Full-text search (`?q=&source=&from=&to=`) |

### Example requests

```bash
# Get the feed (page 1, 20 items)
curl http://localhost:5000/feed

# Filter by source
curl "http://localhost:5000/feed?source=hackernews&size=10"

# Search
curl "http://localhost:5000/search?q=LLM&source=arxiv"

# Daily digest — top 5 per source
curl http://localhost:5000/digest/daily

# Trigger a manual refresh
curl -X POST http://localhost:5000/feed/refresh

# Check HackerNews source health
curl http://localhost:5000/sources/hackernews/health
```

---

## Arkn packages used

| Package | Role |
|---|---|
| `Arkn.Results` | `Result<T>` — explicit success/failure for all source adapters |
| `Arkn.Jobs` | `RefreshFeedJob` (*/30 cron) + `CleanupJob` (daily 03:00 UTC) |
| `Arkn.Logging` | Structured logging with console sink |
| `Arkn.Http` | Typed HTTP clients for each source adapter |
| `Arkn.Core` | Domain primitives |

---

## Tech stack

- **.NET 10** / ASP.NET Core Minimal API
- **EF Core 9** + **SQLite** (zero-config persistence)
- **Arkn** framework (Results, Jobs, Logging, Http)
- **ASP.NET Core Rate Limiting** (native, no middleware package)

---

## Built with Arkn

This project demonstrates how [Arkn](https://github.com/fernando-terra/arkn) enforces explicit patterns in a real API:

- **No exceptions for business logic** — every source adapter returns `Result<T>`
- **No silent failures** — `RefreshFeedJob` logs each source failure individually
- **Typed HTTP clients** — each adapter gets its own `HttpClient` via DI
- **Background jobs with Result contracts** — `IArknJob.ExecuteAsync` returns `Task<Result>`

---

*Author: [Fernando Terra](https://github.com/fernando-terra)*
