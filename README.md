
# QueryQuill 📚

### Status: 🚧 _Work in Progress_ (Active Backend Core Development)

QueryQuill is a full-stack, RAG-based academic study assistant designed for researchers and students. Built with C# & ASP.NET Core, it streamlines the interrogation of complex scholarly materials by providing an effortless ingestion-to-query experience: drop a structured paper (`.md` or `.tex`), let the system index it, and ask away without context-window limitations or AI hallucination.

## 🎯 Problem & Scope
Academic literature is dense, and standard LLM context windows or basic search tools fail to handle scholarly depth efficiently. QueryQuill provides a localized, high-performance semantic search and RAG workspace with a pragmatic MVP scope:

* Streamlined Academic Ingestion: Focused MVP parsing for clean, structured formats like Markdown (`.md`) and LaTeX (`.tex`), leveraging their programmatic structure to build semantic chunks without layout friction.
* Semantic Search: High-dimensional similarity matching powered by PostgreSQL and `pgvector` for precise context retrieval and grounding.
* Low-Latency Embedding Caching: Redis caching layer optimizing compute-heavy query embeddings and top-K search results for rapid response times.
* Full-Stack Decoupling: Robust C# backend API structured for a decoupled client interface (with full TypeScript/React UI integration scheduled for subsequent expansion phases).

## 🏗️ Architecture & Solution Structure
The solution strictly adheres to Clean Architecture principles. Visual system models and documentation will be added after an MVP has been produced in the `docs/architecture/` directory.

```text
QueryQuill/
├── src/
│   ├── QueryQuill.Domain/          # Core entities, value objects, & domain logic
│   ├── QueryQuill.Application/     # Use cases, RAG pipelines, DTOs, & interfaces
│   ├── QueryQuill.Infrastructure/  # PostgreSQL/pgvector persistence, Redis cache, & parsing
│   └── QueryQuill.Api/             # ASP.NET Core Web API controllers & endpoints
├── tests/
│   └── QueryQuill.Tests/           # Unit & integration test suites (xUnit)
└── .github/
    └── workflows/                  # Continuous Integration (CI) build and test pipelines
```

## 🛠️ Tech Stack & Tooling

* Framework: ASP.NET Core (C# / .NET 10)
* Database & Vectors: PostgreSQL with `pgvector` extension
* Caching: Redis (Query embedding & search result caching)
* Architecture: Clean Architecture + Strategy Pattern (for document parsing)
* Testing: xUnit / FluentAssertions
* Infrastructure & DevOps: Docker / Docker Compose, GitHub Actions
* IDE & Tooling: JetBrains Rider, Git


## 📋 Development Roadmap & Phase Timeline Overview

* [x] Solution setup following Clean Architecture multi-project structure (.NET 10)
* [x] Docker Compose configuration for PostgreSQL (`pgvector`) and Redis
* [ ] Domain entity design for academic documents, text chunks, and vector embeddings
* [ ] Strategy Pattern implementation for `.md` and `.tex` parsing pipelines
* [ ] Vector embedding generation and pgvector integration via EF Core
* [ ] Redis caching layer for hot query embeddings and top-K search results
* [ ] REST API endpoints for document ingestion and RAG-grounded querying
* [ ] Unit & integration test suite (xUnit) covering parsing and vector retrieval
* [ 🚀 ] Future Scope: Full-stack TypeScript/React UI integration and expanded PDF parsing strategies

```
[ Phase 1: Repositories & Vector Search ]
                   │
                   ▼
[ Phase 2: Ingestion & Chunking Pipeline ]
                   │
                   ▼
[ Phase 3: RAG Orchestrator & LLM Integration ]
```

## ⚡ Engineering Philosophy

* Radical Simplicity on the Outside: Drop a paper, wait a second, ask your questions—frictionless user workflows backed by rigorous systems design.
* Rigorous Performance Optimization: Leveraging Redis and `pgvector` to eliminate compute bottlenecks and maintain lightning-fast response times during academic discovery.
* Clean Separation of Concerns: Core domain logic isolated completely from database persistence, caching layers, and external parsers.


## 🚀 Quickstart & Local Environment Setup

This project uses a zero-configuration "clone-and-run" local workflow. All required tooling (like `dotnet-ef`) is pinned locally via `.config/dotnet-tools.json`, eliminating the need to install global dependencies.

### Prerequisites

* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Docker Desktop](https://www.docker.com/) or Docker CLI with WSL 2
* (Optional) Trust .NET local HTTPS dev certificates:
```bash
  dotnet dev-certs https --trust
```

---

### Step-by-Step Setup

#### 1. Clone the Repository
```bash
git clone https://github.com/JacobN17/QueryQuill
cd QueryQuill
```

#### 2. Start the PostgreSQL Container

Spin up the local PostgreSQL database instance with `pgvector` enabled:

```bash
docker compose up -d

```

#### 3. Restore Local .NET CLI Tools

Restore the pinned local repository tools (including `dotnet-ef`) specified in `.config/dotnet-tools.json`:

```bash
dotnet tool restore

```

#### 4. Apply Database Migrations (Optional)

Migrations are automatically applied on startup in Development mode (Step 5). If you prefer to apply them manually via CLI:

```bash
dotnet ef database update --project src/QueryQuill.Infrastructure --startup-project src/QueryQuill.Api

```

#### 5. Run the Application

Start the Web API host:

```bash
dotnet run --project src/QueryQuill.Api --launch-profile https

```

The API will be available at https://localhost:7001 (HTTP: http://localhost:5000).

---

### Verifying Setup with Tests

To ensure your container, EF Core mappings, and database connectivity are configured properly, run the test suite:

```bash
dotnet test

```

### Stopping the Environment

To stop the database container:

```bash
docker compose down

# To reset the database and start fresh:
docker compose down -v
```

---

## 📄 License

Distributed under the MIT License. See LICENSE for details.
    
