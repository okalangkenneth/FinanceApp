# FinanceApp — Inherited Project Rehabilitation (CLAUDE-v4)

## Project Identity
- **Type**: Inherited / abandoned (last active ~2023, .NET 5 era)
- **Location**: `E:\Projects\inherited\FinanceApp`
- **Repo**: https://github.com/okalangkenneth/FinanceApp — default branch is **master**
- **What it is**: ASP.NET Core MVC personal finance manager. Identity auth,
  EF Core, transactions/goals/reports, SendGrid email, OpenAI spending analysis,
  PDF export (DinkToPdf), Excel export (EPPlus). Formerly deployed to Heroku (dead).
- **Goal**: Full rehabilitation to production quality per the standard pipeline,
  ending with README rewrite + LinkedIn post. Economics-background framing:
  personal finance + AI-driven spending insight.

## Rehabilitation Pipeline (phase ends = conventional commit + `git push origin master`)
- **Phase 0** — Discovery audit: file-by-file read, numbered debt inventory (IN PROGRESS)
- **Phase 1** — Debt resolution: net8.0 upgrade, dependency cleanup, security fixes,
  PostgreSQL-everywhere migration, AI service replacement
- **Phase 2** — Dockerization (multi-stage net8.0, docker-compose with PostgreSQL)
- **Phase 3** — Service wiring & configuration hygiene
- **Phase 4** — Kubernetes manifests (local cluster, registry :5555)
- **Phase 5** — CI/CD: rewrite GitHub Actions (Heroku target is dead)
- **Phase 6** — Demo: MVC app ⇒ GitHub Pages won't work; decide recorded demo
  + screenshots vs small cloud deployment. DECISION PENDING.
- **Phase 7** — README rewrite + LinkedIn post

## Anti-Hallucination Protocol
- API/library question → verify against docs/source FIRST
- File content question → read the file FIRST
- Never invent function signatures, guess package versions, or assume API behavior
- Confidence levels on every technical claim: HIGH (verified) / MEDIUM (training,
  unverified — flag it) / LOW (must verify) / UNKNOWN (say so, don't guess)
- Read files with `read_multiple_files` (absolute Windows paths, double-backslash)
  BEFORE any edit

## Known Debt — VERIFIED INVENTORY (Phase 0 complete, 2026-06-10)
Full evidence (file:line for every item) in **docs/PHASE0-AUDIT.md**. 49 items.
Seed items 1–17 all confirmed except item 8 (corrected: wkhtmltox binaries are
ABSENT from the tree — PDF export crashes at runtime, not just unmaintained).

### CRITICAL (fix first in Phase 1)
- **A0. Working tree is not a git repo** — no `.git/`; `FinanceApp.git/` is a
  bare MIRROR clone of the GitHub repo. Reconnected during Phase 0 commit.
- **18. ExportController: no [Authorize] + exports ALL users' transactions**
  (no UserId filter) — anonymous cross-tenant data leak
- **19. IDOR in TransactionsController** Edit/Delete — no ownership checks;
  Edit POST reassigns UserId (record hijack); DeleteConfirmed NREs on stale id
- **20. IDOR in FinancialGoalsController** Delete/UpdateFinancialGoal — same

### HIGH — security
- 21. EmailController.SendTestEmail: anonymous SendGrid send (abuse vector),
  hardcoded recipient, missing view → delete controller
- 22. Missing [Authorize]: DashboardController, SpendingAnalysisController
  (anonymous users can trigger paid AI calls)
- 23. UpdateFinancialGoal POST missing [ValidateAntiForgeryToken]
- 24. Login: user enumeration ("User not found.") + lockoutOnFailure:false [MEDIUM]
- 25. HomeController.LogOutAndRedirect: logout over GET (CSRF) [MEDIUM]

### HIGH — platform/dependencies (seed items, all confirmed)
- 1. net5.0 everywhere (csproj ×2, Dockerfile) → net8.0
- 2. OpenAIService: dead legacy completions API; `OpenAI:Endpoint` config key
  defined NOWHERE — feature cannot work at all → rewrite (see Decisions)
- 3. FinanceApp.git/ purge via git filter-repo (coordinate force-push)
- 4. Heroku artifacts (heroku.yml w/ foreign puma/python blocks, Pocfile,
  dead demo URL, dead workflow) → remove
- 5. DI bug: OpenAIService AddHttpClient + AddSingleton double registration
- 6. Identity config contradiction (RequireConfirmedAccount true then false) —
  root cause is item 26; collapses to `true` once 26 is fixed
- 7. Package debt: Identity 2.2.0, System.Net.Http 4.3.4, Npgsql.Design 1.1.0,
  mixed Extensions 6.0.0/5.0.17, CodeGeneration.Design
- 8. DinkToPdf wired but wkhtmltox/ dir ABSENT → PDF export throws today;
  replace with QuestPDF (license check at adoption)
- 10. Dual DB providers; 35. forced nvarchar(MAX) on indexed Identity columns
  breaks SQL Server schema creation; 36. the single migration is Npgsql-only —
  SQL Server dev path is entirely broken → PostgreSQL everywhere
- 12. Newtonsoft.Json used but not referenced → System.Text.Json in rewrite
- 13. Duplicate appsettings.Production.json (root copy dead; project copy has
  `#{HEROKU_CONNECTION_STRING}#` token placeholder)
- 14. Startup/Program split → net8 minimal hosting

### HIGH — correctness
- 26. **ApplicationUser.EmailConfirmed SHADOWS IdentityUser.EmailConfirmed**
  (CS0108) — causes manual ConfirmEmail workaround + item 6; delete property
- 27. **Reports classify income/expense by Category, not TransactionType** —
  Reports and Dashboard disagree on the same data (financial correctness)
- 28. HomeController.Dashboard: totals computed from last 5 transactions only
  + view missing (500) — duplicate of DashboardController; delete
- 39. CI workflow: no build/test step, deploys to dead Heroku, checkout@v2 →
  Phase 5 rewrite
- 40. Test suite = ONE empty test ([Fact] with empty body); 0% coverage
- 41. Config keys consumed but defined nowhere: SendGrid:ApiKey, OpenAI:ApiKey,
  OpenAI:Endpoint; appsettings.Development.json currently COMMITTED (no secrets
  in it yet — remove from repo before adding any)

### MEDIUM
- 9. EPPlus 5.7.5 Polyform Noncommercial → swap to ClosedXML (MIT)
- 11. README false claims: SignalR (zero usage anywhere — chart is static
  Chart.js pie of spending, not real-time net worth), "XSS/CSRF protected"
  (contradicted by 18–23), CI "build/test" (no such steps), dead demo URL
- 15. DockerDefaultTargetOS=Windows vs Linux Dockerfile (VS tooling only) — Phase 2
- 16. CloneHttpRequestMessage dead code + ALSO the entire
  HttpRequestMessageExtensions.Clone() extension class is dead (.Result in both) → delete
- 29. FinancialGoal.Progress divides by TargetAmount, no zero guard (decimal
  division throws)
- 30. AccountController.Profile view missing (500)
- 31. Orphan view Views/FinancialGoals/Edit.cshtml posts to nonexistent action
- 32. Misplaced Razor Pages LoginModel in Views/Shared/Login.cshtml.cs —
  duplicate login flow, references nonexistent pages → delete
- 34. Port config three ways: Program.cs ListenAnyIP(PORT??10000), appsettings
  Kestrel 8888, launchSettings 10000 — likely binds BOTH 8888 and 10000
  [MEDIUM confidence]; standardize on 8888 in Phase 3
- 37. Money columns `numeric` without precision → numeric(18,2) on regeneration
- 38. `timestamp without time zone` + DateTime.Now — WILL break on Npgsql 8
  upgrade (UTC enforcement); decide UTC strategy up front in Phase 1
- 42. Hardcoded: sender/recipient ken@backendinsight.com, seed user GUID,
  OpenAI base URL
- 44. Transaction has BOTH PreferredCurrency string AND Currency enum;
  CurrencyHelper formats SEK/USH as GBP (wrong symbol in a finance app)

### LOW
- 17. SeedData commented out, hardcoded GUID, goals missing StartDate
- 33. Phantom Identity area route; RazorPages registered with no pages
- 43. SendGridClient per call; send result ignored
- 45. OpenAI retry loop retries non-transient errors; `throw ex` (moot after rewrite)
- 46. ILogger<AccountController> injected into SpendingAnalysisController
- 47. Create flow reuses UpdateFinancialGoalViewModel (naming)

### Verified clean ✅
- Money CLR types: `decimal` everywhere (Transaction.Amount, FinancialGoal
  Target/CurrentAmount, all view models) — no float/double
- No secrets anywhere in the tree (both HerokuConnection values empty/token)
- CSRF tokens on all forms (tag helpers); only gap is item 23
- No raw user input rendered (Html.Raw uses are Json.Serialize of enums/decimals)
- No raw SQL; all read paths filter on UserId (IDOR is write/delete/export only)

## Critical Rules
- **Money handling (NON-NEGOTIABLE)**: all monetary amounts as `decimal` in C#
  (never `float`/`double`); EF Core column type `numeric` with explicit
  precision; never parse money with float semantics. Audit existing Transaction/
  Goal models for this in Phase 0.
- NO placeholders (`YOUR_API_KEY`, `TODO`) in committed code
- Secrets via `appsettings.Development.json` (gitignored) for `dotnet run`;
  `.env` is NOT auto-loaded by the .NET runtime
- Only modify what was explicitly requested; ask if <90% confident
- Never `docker restart` — always `docker compose up -d --build [service]`

## Tech Stack (target state)
- .NET 8 (net8.0), ASP.NET Core MVC, minimal hosting model
- EF Core 8 + Npgsql (PostgreSQL everywhere — dev and prod)
- ASP.NET Core Identity 8
- System.Text.Json (no Newtonsoft)
- AI: Anthropic API or self-hosted Dify workflow (replaces OpenAI) — DECISION PENDING
- PDF: QuestPDF (replaces DinkToPdf) — pending license check
- Excel: ClosedXML or EPPlus (pending license check)
- Docker multi-stage, docker-compose, Kubernetes (local), GitHub Actions

## Ports (check landscape before assigning)
- Occupied elsewhere: 5000–5004, 5050 (gateway), 5433 (PostgreSQL), 5555 (registry)
- App port config is contradictory (audit item 34): appsettings Kestrel says
  8888, Program.cs ListenAnyIP(PORT??10000), launchSettings 10000 — likely
  binds both. Standardize on 8888 (free) in Phase 3
- FinanceApp PostgreSQL container: use 5434 to avoid the 5433 conflict

## Workflow
```
1. Make changes
2. Build:  dotnet build FinanceApp.sln
3. Test:   dotnet test FinanceApp.sln
4. Run:    dotnet run --project FinanceApp (secrets via appsettings.Development.json)
5. Commit: conventional commits (feat:, fix:, chore:, refactor:, docs:)
6. Push:   git push origin master   ← EVERY phase end, branch is master NOT main
```

## Git Conventions
- Default branch: **master**. Every Claude Code prompt for this repo ends with
  `git push origin master`.
- Conventional commits; messages are public portfolio surface — keep descriptive
- Never commit: appsettings.Development.json with secrets, .env, API keys
- History surgery (FinanceApp.git purge) happens in Phase 1 with `git filter-repo`
  — coordinate before force-push

## Build Progress (KEEP UPDATED — Claude Code: update at end of every session)

### ✅ COMPLETED
- Pre-Phase 0 reconnaissance (seed debt list, items 1–17)
- Secrets exposure check: clear
- **Phase 0: full discovery audit (2026-06-10)** — 49-item verified inventory
  in docs/PHASE0-AUDIT.md; seed list verified (16 confirmed, item 8 corrected);
  3 CRITICAL security findings (anonymous data export, 2× IDOR); working tree
  reconnected to GitHub (no .git existed — only the FinanceApp.git bare mirror)

### 🔨 IN PROGRESS
- Awaiting user sign-off on two Phase 1 decisions (recommendations in
  docs/PHASE0-AUDIT.md §4): AI replacement → Anthropic API direct;
  demo strategy → recorded demo + screenshots

### ❌ REMAINING
- Phases 1–7 per pipeline above

## Correction Log
When corrected, append to `docs/claude-corrections.md` (Mistake / Correction /
Rule). Read it at session start before any work.

## Session Management
- Start: read docs/claude-corrections.md if present; trust CLAUDE.md context
- During /compact, always preserve: modified file paths, current branch +
  uncommitted changes, pending tasks, test failures, key decisions
- End: update Build Progress above, conventional commit, `git push origin master`
