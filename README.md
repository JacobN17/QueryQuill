# QueryQuill

A multi-layer Retrieval-Augmented Generation (RAG) engine built in .NET 10 following Clean Architecture principles.

## Intent & Purpose
Developed to explore enterprise RAG pipeline architectures and RESTful service patterns within the .NET ecosystem—emphasizing strict Clean Architecture boundaries, modular vector retrieval, industry-standard Web API design, and automated test evaluation.

## Tech Stack
- **Framework:** .NET 10 / C#
- **Database:** PostgreSQL with `pgvector`
- **Architecture:** Clean Architecture (`Api`, `Core`, `Domain`, `Infrastructure`)
- **Containerization:** Docker & Docker Compose

## Project Status
Work in Progress 

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
    
