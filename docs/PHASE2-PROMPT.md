# Phase 2 Prompt — paste into Claude Code at E:\Projects\inherited\FinanceApp

Phase 1 (A–E) is complete; history was rewritten, all hashes changed. Read
CLAUDE.md first. Phase 2 = Dockerization. Docker Desktop is running.

## Step 0 — Pre-flight cleanup
1. Verify the working tree contains NO FinanceApp.git/ directory; if present,
   delete it and confirm .gitignore would exclude it (add a line if not).
   It must never be re-committed — the history purge depends on this.
2. `git status` must be clean before starting.

## Step 1 — docker-compose.yml at repo root
1. Services: `app` (build from Dockerfile) and `db` (postgres:16-alpine)
2. db: host port 5434 → container 5432 (5433 is occupied by another project);
   named volume for data; healthcheck via pg_isready; POSTGRES_* env vars
3. app: port 8888:8080 (net8.0 default container port is 8080 — verify the
   Dockerfile's ASPNETCORE_HTTP_PORTS rather than assuming);
   depends_on db with condition: service_healthy
4. Configuration via environment in compose:
   ConnectionStrings__DefaultConnection pointing at db:5432 (service name,
   container port — NOT localhost:5434, that's only for host-side dotnet run),
   Anthropic__ApiKey passed through from a gitignored .env file next to
   docker-compose.yml. Note: .env works fine here — compose reads it;
   the earlier ".env not auto-loaded" rule applies to the .NET runtime only.
5. Commit nothing containing a real key; .env goes in .gitignore, ship a
   .env.example with placeholder names only (placeholder names are fine in
   an example file; never in code or real config).

## Step 2 — Dockerfile review (written in 1B, never run)
1. Build it for real: `docker compose build app`. Fix what fails.
2. Add a non-root user in the final stage; verify QuestPDF and ClosedXML
   need no native packages in the runtime image (QuestPDF bundles SkiaSharp
   natives — if the alpine/chiseled base breaks it, use the standard
   aspnet:8.0 Debian-based image and note why in a comment)
3. Confirm .dockerignore excludes bin/, obj/, docs/, .git, .env, *.bundle

## Step 3 — Database: first real migration run
1. `docker compose up -d db`, wait for healthy
2. Apply migrations against it. Choose ONE strategy and implement it:
   recommended = a migration apply on app startup gated behind an
   env flag (RUN_MIGRATIONS=true in compose for dev; document that
   production would use a migration job instead). The chain was only
   verified offline in 1D — this is the first live `database update`;
   if it fails, fix the migration in a follow-up migration, never edit
   applied ones.
3. Verify schema: tables exist, money columns are numeric(18,2),
   timestamps are timestamptz (1D's converter strategy)

## Step 4 — Health endpoint + full stack smoke test
1. Add AddHealthChecks + AspNetCore.HealthChecks.Npgsql, map /health
2. `docker compose up -d --build` (NEVER `docker restart` — stale binaries)
3. Smoke test and SHOW me the output: curl /health returns Healthy;
   register a user via the UI flow or directly confirm Identity tables
   populate; app logs clean of errors/warnings on startup
4. Tests still pass on host: `dotnet test FinanceApp.sln`

## Step 5 — Wrap up
1. README: do NOT rewrite yet (Phase 7), but add a minimal "Run locally"
   section: cp .env.example .env, add key, docker compose up
2. Update CLAUDE.md Build Progress (Phase 2 → COMPLETED, note the
   migration strategy decision and any image-size/base-image decisions)
3. Commit: `feat: dockerize app + PostgreSQL with health checks (Phase 2)`
   then `git push origin master`
