# 🤖 AIFeed

> A minimal, production-ready AI news broker — aggregates content from multiple sources into a unified feed with a React frontend and a .NET 10 API.

[![Built with Arkn](https://img.shields.io/badge/built%20with-Arkn-6366f1?style=flat-square)](https://github.com/fernando-terra/arkn)
[![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com)
[![React](https://img.shields.io/badge/React-19-61DAFB?style=flat-square&logo=react)](https://react.dev)
[![Vite](https://img.shields.io/badge/Vite-6-646CFF?style=flat-square&logo=vite)](https://vitejs.dev)
[![License: MIT](https://img.shields.io/badge/license-MIT-green?style=flat-square)](LICENSE)

---

## What it does

AIFeed continuously ingests AI-related content from five sources, normalises it into a unified schema, and serves it through a clean REST API and a React UI with search, pagination, and per-source filtering.

| Source | Content | Auth required |
|---|---|---|
| **Hacker News** | Discussions and links | ❌ |
| **dev.to** | Technical articles | ❌ |
| **arXiv** | Research papers (cs.AI) | ❌ |
| **GitHub Trending** | AI repositories | ❌ (optional token for higher rate limit) |
| **Product Hunt** | AI product launches | ✅ `PRODUCTHUNT_TOKEN` |

---

## Monorepo structure

```
aifeed/
├── apps/
│   ├── api/           → .NET 10 REST API (Vertical Slice Architecture)
│   └── web/           → React 19 + Vite frontend
├── nginx/
│   └── default.conf   → Serves the React app + proxies /api/* to the .NET API
├── Dockerfile.api     → Multi-stage build for the .NET API
├── Dockerfile.web     → Multi-stage build for React + nginx
├── docker-compose.yml → Orchestrates aifeed-api + aifeed-web
└── .env.example       → Optional token configuration
```

---

## Architecture

### API (`apps/api`)

**Vertical Slice Architecture** — each feature owns its own request/response logic with no shared application layer.

```
apps/api/
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

### Frontend (`apps/web`)

React 19 + Vite app served by nginx. The API is consumed via relative paths (`/api/*`) — nginx handles the proxy internally. No CORS, no public API exposure.

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/feed` | Paginated feed (`?page=&size=&source=`) |
| `POST` | `/api/feed/refresh` | Trigger immediate refresh |
| `GET` | `/api/sources` | List all sources |
| `GET` | `/api/sources/{id}/health` | Health check one source |
| `GET` | `/api/digest/daily` | Today's top items |
| `GET` | `/api/digest/weekly` | This week's top items |
| `GET` | `/api/search` | Search by keyword, source, date range |
| `GET` | `/api/health` | API health check |

---

## Rate Limiting

- **Spike arrest** — global 20 req/s sliding window
- **Per-IP** — 100 req/min sliding window

Exceeded limits return `429 Too Many Requests`.

---

## Running with Docker

```bash
git clone https://github.com/fernando-terra/aifeed
cd aifeed

# Configure optional tokens
cp .env.example .env
# edit .env with GITHUB_TOKEN and/or PRODUCTHUNT_TOKEN

docker compose up -d
```

The app will be available at `http://localhost:8181`.
The SQLite database is persisted in the `aifeed-data` Docker volume.

```bash
# View logs
docker compose logs -f

# Stop
docker compose down

# Rebuild after code changes
docker compose up -d --build
```

---

## Running locally (dev)

**Prerequisites:** .NET 10 SDK, Node.js 22+

### API

```bash
cd apps/api
dotnet run
# API available at http://localhost:5000
```

### Frontend

```bash
cd apps/web
npm install
npm run dev
# Dev server at http://localhost:5173 (proxies /api/* to localhost:5000)
```

**Optional environment variables:**

```bash
GITHUB_TOKEN=ghp_...       # raises GitHub API limit from 60 to 5000 req/h
PRODUCTHUNT_TOKEN=...      # enables Product Hunt source
```

---

## How Arkn is used

| Package | Usage |
|---|---|
| `Arkn.Results` | Every source adapter returns `Result<T>` — failures are explicit and composable |
| `Arkn.Http` | Typed `HttpClient` wrappers per source via `IHttpClientFactory` |
| `Arkn.Jobs` | `RefreshFeedJob` (cron: `*/30 * * * *`) + `CleanupJob` (cron: `0 3 * * *`) |
| `Arkn.Logging` | Structured console logging across jobs and endpoints |

---

## License

MIT — see [LICENSE](LICENSE).
