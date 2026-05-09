# 🤖 AIFeed

> A minimal, production-ready AI news broker built with [Arkn](https://github.com/fernando-terra/arkn) — aggregates content from multiple sources into a single, clean API.

[![Built with Arkn](https://img.shields.io/badge/built%20with-Arkn-6366f1?style=flat-square)](https://github.com/fernando-terra/arkn)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![License: MIT](https://img.shields.io/badge/license-MIT-green?style=flat-square)](LICENSE)

---

## What it does

AIFeed continuously ingests AI-related content from five sources, normalises it into a unified schema, and exposes a simple REST API with pagination, search, and daily/weekly digests.

| Source | Content | Auth required |
|---|---|---|
| **Hacker News** | Discussions and links | ❌ |
| **dev.to** | Technical articles | ❌ |
| **arXiv** | Research papers (cs.AI) | ❌ |
| **GitHub Trending** | AI repositories | ❌ (optional token for higher rate limit) |
| **Product Hunt** | AI product launches | ✅ `PRODUCTHUNT_TOKEN` |

---

## Architecture

AIFeed uses **Vertical Slice Architecture** — each feature lives in its own folder and owns its own request/response logic. There is no shared application layer.

```
AIFeed.Api/
├── Features/
│   ├── Sources/       → GET /sources, GET /sources/{id}/health
│   ├── Feed/          → GET /feed, POST /feed/refresh
│   ├── Digest/        → GET /digest/daily, GET /digest/weekly
│   └── Search/        → GET /search
└── Infrastructure/
    ├── Persistence/   → EF Core + SQLite (AppDbContext, FeedItem)
    ├── Sources/       → One adapter per source (IFeedSource)
    └── Jobs/          → RefreshFeedJob (every 30 min) + CleanupJob (daily)
```

The `IFeedSource` contract ensures each adapter is independently replaceable. A failing source returns `Result.Failure<T>` — it never crashes the feed.

---

## Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/feed` | Paginated feed (`?page=&size=&source=`) |
| `POST` | `/feed/refresh` | Trigger immediate refresh |
| `GET` | `/sources` | List all sources |
| `GET` | `/sources/{id}/health` | Health check one source |
| `GET` | `/digest/daily` | Today's top items |
| `GET` | `/digest/weekly` | This week's top items |
| `GET` | `/search` | Search by keyword, source, date range |
| `GET` | `/health` | API health check |

---

## Rate Limiting

The API is public with two protection layers:

- **Spike arrest** — global 20 req/s sliding window
- **Per-IP** — 100 req/min sliding window

Exceeded limits return `429 Too Many Requests`.

---

## Running locally

**Prerequisites:** .NET 10 SDK

```bash
git clone https://github.com/fernando-terra/aifeed
cd aifeed/AIFeed.Api
dotnet run
```

The SQLite database is created automatically on first run (`aifeed.db`).

**Optional environment variables:**

```bash
GITHUB_TOKEN=ghp_...          # raises GitHub API limit from 60 to 5000 req/h
PRODUCTHUNT_TOKEN=...         # enables Product Hunt source
```

### Trigger a manual refresh

```bash
curl -X POST http://localhost:5000/feed/refresh
```

### Query the feed

```bash
# Latest 20 items
curl http://localhost:5000/feed

# Filter by source
curl "http://localhost:5000/feed?source=arxiv&size=10"

# Search
curl "http://localhost:5000/search?q=llm"
```

---

## How Arkn is used

This project showcases several [Arkn](https://github.com/fernando-terra/arkn) packages working together:

| Package | Usage |
|---|---|
| `Arkn.Results` | Every source adapter returns `Result<T>` — failures are explicit and composable |
| `Arkn.Http` | Typed `HttpClient` wrappers per source via `IHttpClientFactory` |
| `Arkn.Jobs` | `RefreshFeedJob` (cron: `*/30 * * * *`) + `CleanupJob` (cron: `0 3 * * *`) |
| `Arkn.Logging` | Structured console logging across jobs and endpoints |

---

## License

MIT — see [LICENSE](LICENSE).
