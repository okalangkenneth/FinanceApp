# Phase 0 — Discovery Audit (FinanceApp)

**Date**: 2026-06-10
**Scope**: Full read of every source file (Controllers, Models, Services, Data,
Helpers, Exceptions, Migrations, Views, Tests, root artifacts, CI). Vendor files
under `wwwroot/lib` and `FinanceApp.git/` object contents skipped per protocol.
**Method**: Every claim below carries file:line evidence. Confidence is HIGH
(verified by reading the file) unless flagged otherwise.

---

## 0. Process-blocking discovery

### A0. Working tree is NOT a git repository — CRITICAL (process)
- `git status` → `fatal: not a git repository`. There is no `.git/` at
  `E:\Projects\inherited\FinanceApp`.
- `FinanceApp.git/` at the root is a **bare mirror clone** of
  `https://github.com/okalangkenneth/FinanceApp.git`
  (`FinanceApp.git\config`: `bare = true`, `mirror = true`, `HEAD` →
  `refs/heads/master`). Confirmed bare: contains `HEAD`, `config`, `objects/`,
  `packed-refs`, no working tree.
- Consequence: no phase-end commit/push is possible until the working tree is
  reconnected (`git init -b master` + `remote add origin` + `fetch` +
  `reset --mixed origin/master`). Done as part of this phase's deliverable
  commit. The `FinanceApp.git/` directory itself remains debt item 3 (history
  purge in Phase 1).

---

## 1. Seed list verification (CLAUDE.md items 1–17)

### 1. net5.0 target — CONFIRMED — HIGH — Phase 1
- `FinanceApp\FinanceApp.csproj:4` — `<TargetFramework>net5.0</TargetFramework>`
- `FinanceApp.Tests\FinanceApp.Tests.csproj:4` — also net5.0
- `Dockerfile:6,11` — `aspnet:5.0` / `sdk:5.0` images
- .NET 5 EOL 2022-05-10. Upgrade everything to net8.0.

### 2. OpenAIService uses dead legacy completions API — CONFIRMED — HIGH — Phase 1
- `FinanceApp\Services\OpenAIServices.cs:34,112` — parses
  `jsonResponse["choices"][0]["text"]` (legacy Completions shape)
- `:50` — request body `{ prompt, max_tokens = 150 }` (legacy schema)
- `:29,107` — endpoint passed in per-call as a string, sourced from config key
  `OpenAI:Endpoint` (`SpendingAnalysisController.cs:60,111`) which is **defined
  nowhere** (see item 41). The feature cannot work at all today.
- Replacement decision: see §4 Recommendations.

### 3. `FinanceApp.git/` bare repo committed at root — CONFIRMED — HIGH — Phase 1
- Bare mirror at repo root (see A0). Purge from history with `git filter-repo`
  in Phase 1; coordinate before force-push.

### 4. Heroku artifacts dead — CONFIRMED — HIGH — Phase 1 (remove) / 5 (replace)
- `heroku.yml:6` — `bundle exec puma -C config/puma.rb` (Ruby, foreign to this app)
- `heroku.yml:9` — `python myworker.py` (Python, foreign to this app)
- `Pocfile:1` — typo'd Procfile (`web: dotnet FinanceApp.dll`)
- `README.md:31` — dead demo URL `fin-trak.herokuapp.com`
- `.github\workflows\heroku_deploy.yml` — deploys to dead Heroku app (see item 39)
- `appsettings.Production.json` (both copies) — `HerokuConnection` string

### 5. DI bug: OpenAIService double registration — CONFIRMED — HIGH — Phase 1
- `FinanceApp\Startup.cs:88` — `AddHttpClient<OpenAIService>()` (typed client)
- `Startup.cs:89` — `AddSingleton(x => new OpenAIService(x.GetRequiredService<HttpClient>(), ...))`
  overrides the typed registration; resolves the *default unnamed* `HttpClient`,
  not the typed-client pipeline, and pins it in a singleton for the process
  lifetime (defeats `IHttpClientFactory` handler rotation).
- API key duplicated: passed in ctor (`Startup.cs:89`) AND per call
  (`OpenAIServices.cs:29,107`, fed from `SpendingAnalysisController.cs:59,110`).

### 6. Contradictory Identity config — CONFIRMED — HIGH — Phase 1
- `Startup.cs:59` — `AddIdentity(... options.SignIn.RequireConfirmedAccount = true)`
- `Startup.cs:63` — immediately overridden:
  `services.Configure<IdentityOptions>(options => options.SignIn.RequireConfirmedAccount = false)`
- Net effect: framework does NOT require confirmation; instead
  `AccountController.cs:48` manually gates login on the **shadowed**
  `EmailConfirmed` property (see item 26).

### 7. Package debt — CONFIRMED — HIGH — Phase 1
- `FinanceApp.csproj:13` — `Microsoft.AspNetCore.Identity 2.2.0` (legacy 2.x
  package; must not be referenced on net5+)
- `:25` — `System.Net.Http 4.3.4` (unnecessary explicit ref, old CVE surface)
- `:23` — `Npgsql.EntityFrameworkCore.PostgreSQL.Design 1.1.0` (ancient, dead)
- `:18,19` — `Microsoft.Extensions.Configuration/Http 6.0.0` mixed with 5.0.17
  packages
- `:21` — `Microsoft.VisualStudio.Web.CodeGeneration.Design 5.0.2` (scaffolding,
  dev-only; remove or update)

### 8. DinkToPdf + wkhtmltox — CONFIRMED, WORSE THAN SEEDED — HIGH — Phase 1
- `FinanceApp.csproj:10` — DinkToPdf 1.0.8; `:29-31` — csproj copies
  `wkhtmltox\**\*` to output; `Startup.cs:98-108` — native resolver loads
  `wkhtmltox/<arch>/libwkhtmltox`.
- **Correction to seed list**: the `wkhtmltox\` directory does **not exist** in
  the working tree (verified: zero files). PDF export
  (`ExportController.ExportTransactionsReportPdfAsync`) crashes at runtime with
  a native load failure today. Binaries may still exist in git history (check
  during Phase 1 filter-repo pass).
- Replace with QuestPDF (license check pending — Community tier is free for
  small companies; verify current terms at adoption time).

### 9. EPPlus 5.7.5 licensing — CONFIRMED — MEDIUM — Phase 1
- `FinanceApp.csproj:11`; `ExportController.cs:29` sets
  `ExcelPackage.LicenseContext = LicenseContext.NonCommercial`.
- EPPlus ≥5 is Polyform Noncommercial. Acceptable for a personal portfolio,
  but ClosedXML (MIT) removes the question entirely. Recommend swap.

### 10. Dual DB providers — CONFIRMED — HIGH — Phase 1
- `Startup.cs:43-53` — SqlServer in Development, Npgsql otherwise.
- `Data\ApplicationDbContext.cs:22-41` — provider-sniffing in
  `OnModelCreating`, forcing `nvarchar(MAX)` (SQL Server) / `text` (PG) on
  **every** string column.
- `Properties\serviceDependencies.json` / `.local.json` — mssql wiring.
- See items 35–36: the SQL Server path is actually **broken**, which makes
  "PostgreSQL everywhere" the only sane direction.

### 11. README/reality mismatch — CONFIRMED & EXPANDED — MEDIUM — Phase 7
README claims not backed by code:
- **SignalR real-time chart** (`README.md:38,59`): zero SignalR usage — no
  package in csproj, no hub classes, no `MapHub`, no client script (verified by
  repo-wide grep). The dashboard chart is a **static Chart.js pie chart** of
  spending by category (`Views\Dashboard\Index.cshtml:177-202`, Chart.js from
  CDN at `Views\Shared\_Layout.cshtml:90`) — not a net-worth chart, not
  real-time.
- **"protects against XSS and CSRF"** (`README.md:69`): contradicted by items
  18–23 (anonymous data export, IDOR, missing antiforgery on one POST).
- **CI "build, test, deployment"** (`README.md:75`): workflow has no build or
  test step (item 39); test suite is one empty test (item 40).
- **Live demo** (`README.md:31`): dead Heroku URL.
- Cosmetic: broken numbering (`8.8.` at `README.md:61`, list restarts at 1).
- `@Html.Raw` usage is `Json.Serialize` of enum names/decimals
  (`Views\Dashboard\Index.cshtml:193-197`) — not a real XSS surface.

### 12. Newtonsoft.Json used but not referenced — CONFIRMED — HIGH — Phase 1
- `OpenAIServices.cs:4-5` — `using Newtonsoft.Json(.Linq)`; no PackageReference
  in `FinanceApp.csproj` (resolves transitively). Migrate to System.Text.Json
  during the OpenAIService rewrite.

### 13. Duplicate appsettings.Production.json — CONFIRMED — HIGH — Phase 1/3
- Root copy: `HerokuConnection` = `""` (empty — no secret leak ✅)
- Project copy (`FinanceApp\appsettings.Production.json:3`):
  `"#{HEROKU_CONNECTION_STRING}#"` — a token-replacement placeholder committed
  to source. Only the project-dir copy is actually loaded by the runtime; the
  root copy is dead. Remove both in favor of env-var configuration.

### 14. Startup/Program split — CONFIRMED — HIGH — Phase 1
- `Program.cs` (Generic Host + `UseStartup`) + `Startup.cs`. Migrate to
  net8 minimal hosting (`WebApplicationBuilder`).

### 15. DockerDefaultTargetOS=Windows vs Linux Dockerfile — CONFIRMED — MEDIUM — Phase 2
- `FinanceApp.csproj:6` vs Linux-style `Dockerfile`. Severity downgraded from
  HIGH: the property only steers VS container tooling, not CLI builds. Fix in
  the Phase 2 Dockerfile rewrite.

### 16. CloneHttpRequestMessage `.Result` — CONFIRMED DEAD — MEDIUM — Phase 1
- `OpenAIServices.cs:89-106` — private, **never called** (repo-wide grep:
  definition only). Delete.
- **Additional**: `Services\HttpRequestMessageExtensions\HttpRequestMessageExtensions.cs`
  — the entire `Clone()` extension class is also never called and also uses
  `.Result` (`:13`). Delete the whole folder.

### 17. SeedData commented out — CONFIRMED — LOW — Phase 1
- `Startup.cs:151` — `// SeedData.Seed(dbContext);`
- `Data\SeedData.cs:17,26,45,55` — hardcoded user GUID
  `06c1cda4-3143-448e-bfd7-27b0a2568186`; goals seeded without `StartDate`.
  Decide: proper idempotent seeding keyed to a real dev user, or remove.

### Money handling audit (mandated by CLAUDE.md) — PASS with caveat
- CLR type is `decimal` everywhere: `Models\Transaction.cs:19` (`Amount`),
  `Models\FinancialGoal.cs:12-13` (`TargetAmount`, `CurrentAmount`), plus all
  view models (`IncomeVsExpenseViewModel`, `MonthlyTotal`, `CategoryTotal`,
  `CategorySpending`, `SpendingAnalysis`). No float/double anywhere. ✅
- DB columns are `numeric` (`Migrations\ApplicationDbContextModelSnapshot.cs:105-106,120-121,144-145`)
  but **without explicit precision/scale** → item 37.

---

## 2. New findings (18–49)

### Security — CRITICAL

**18. Anonymous cross-tenant data export — CRITICAL — Phase 1 (immediate)**
- `Controllers\ExportController.cs` — class has **no `[Authorize]`**, and both
  actions export **all users'** transactions:
  - `:32` `ExportTransactionsReportExcel` → `_context.Transactions.ToList()` (no user filter)
  - `:73` `ExportTransactionsReportPdfAsync` → same
- Any unauthenticated visitor can download every user's full transaction
  history as Excel. (PDF path currently 500s due to item 8, which is the only
  thing "protecting" it.)

**19. IDOR — TransactionsController Edit/Delete — CRITICAL — Phase 1**
- `Controllers\TransactionsController.cs:73` (Edit GET), `:84-96` (Edit POST),
  `:123-124` (Delete GET), `:138` (DeleteConfirmed) — lookups by id only, no
  `UserId` ownership check. `[Authorize]` is present but any logged-in user can
  read/modify/delete any other user's transactions. Edit POST (`:95`) even
  reassigns `UserId` to the attacker, stealing the record.
- Also `:139` — `DeleteConfirmed` calls `Remove(transaction)` without a null
  check → NRE/500 on a stale id.

**20. IDOR — FinancialGoalsController — CRITICAL — Phase 1**
- `Controllers\FinancialGoalsController.cs:77-78` (Delete GET), `:92`
  (DeleteConfirmed), `:107` (UpdateFinancialGoal GET), `:135` (POST) — same
  pattern: id-only lookups, no ownership check.

### Security — HIGH

**21. Anonymous email-sending endpoint — HIGH — Phase 1**
- `Controllers\EmailController.cs:19-23` — `SendTestEmail` has no `[Authorize]`;
  anyone can trigger SendGrid sends (cost/abuse vector). Hardcoded recipient
  `ken@backendinsight.com`. Returns `View()` but `Views\Email\` doesn't exist →
  500 after sending. Delete the controller.

**22. Missing `[Authorize]` on DashboardController and SpendingAnalysisController — HIGH — Phase 1**
- `Controllers\DashboardController.cs:13` — relies on a manual
  `userId == null` redirect (`:28-31`).
- `Controllers\SpendingAnalysisController.cs:19` — anonymous users can invoke
  `Analyze`/`Recommendations`, i.e. trigger paid AI API calls (cost abuse) —
  `Recommendations` (`:104`) doesn't even need user data.

**23. Missing `[ValidateAntiForgeryToken]` on UpdateFinancialGoal POST — HIGH — Phase 1**
- `FinancialGoalsController.cs:130-131` — `[HttpPost]` without antiforgery
  validation (every other POST in the app has it). The form emits a token
  (tag helper) but the server never validates it → CSRF on goal modification.

### Security — MEDIUM

**24. Login: user enumeration + no lockout — MEDIUM — Phase 1**
- `AccountController.cs:76` — `"User not found."` vs `:66`
  `"Invalid login attempt."` discloses account existence.
- `:50` — `PasswordSignInAsync(..., lockoutOnFailure: false)` — brute force is
  never locked out (same in `Views\Shared\Login.cshtml.cs:83`).

**25. Logout over GET — MEDIUM — Phase 1**
- `Controllers\HomeController.cs:39-46` — `LogOutAndRedirect` signs the user
  out on a GET with no antiforgery (CSRF logout / prefetch logout). The proper
  POST logout exists in `AccountController.cs:98-104`; delete this one.

### Correctness bugs — HIGH

**26. `ApplicationUser.EmailConfirmed` shadows `IdentityUser.EmailConfirmed` — HIGH — Phase 1**
- `Models\ApplicationUser.cs:12` — re-declares `public bool EmailConfirmed`
  (CS0108 warning; hides the base property Identity writes to). This is why
  `AccountController.ConfirmEmail` (`:121-126`) calls `ConfirmEmailAsync` and
  then *manually* sets the property again. Delete the duplicate property; the
  manual workaround and the `Startup.cs:63` override (item 6) then collapse
  into normal `RequireConfirmedAccount = true` behavior.

**27. Reports classify income/expense by Category, ignoring TransactionType — HIGH — Phase 1**
- `Controllers\ReportsController.cs:46-58` (IncomeVsExpense), `:85-88`
  (CategoryBreakdown), `:125-134` (MonthlyBudget) — "income" = category ∈
  {Salary, Freelance, Investment, Gift}; everything else is "expense".
  A transaction recorded as `Type=Income, Category=Other` (exactly what
  `SeedData.cs:26-31` creates) is counted as an **expense**.
  `DashboardController.cs:41-42` uses `t.Type` — the two screens disagree on
  the same data. Standardize on `TransactionType`.

**28. HomeController.Dashboard: totals from last 5 transactions + missing view — HIGH — Phase 1**
- `Controllers\HomeController.cs:63-67` — `Take(5)`, then `:86-88` sums
  income/expenses/balance over those 5 rows only.
- No `Views\Home\Dashboard.cshtml` exists → the action 500s anyway.
- Duplicate of `DashboardController.Index`. Delete the action.

### Correctness bugs — MEDIUM

**29. Division by zero in `FinancialGoal.Progress` — MEDIUM — Phase 1**
- `Models\FinancialGoal.cs:21` — `CurrentAmount / TargetAmount * 100` with no
  zero guard; `TargetAmount` is user input. `DivideByZeroException` (decimal
  division throws) wherever Progress is rendered.

**30. `AccountController.Profile` view missing — MEDIUM — Phase 1**
- `AccountController.cs:171-183` returns `View(user)`;
  `Views\Account\` contains only `ConfirmEmail.cshtml` → 500.

**31. Orphan view `Views\FinancialGoals\Edit.cshtml` — MEDIUM — Phase 1**
- `:8` posts to `asp-action="Edit"`; `FinancialGoalsController` has **no Edit
  action** (only `UpdateFinancialGoal`) → 404 on submit. Delete or wire up.

**32. Misplaced Razor Pages `LoginModel` in Views\Shared — MEDIUM — Phase 1**
- `Views\Shared\Login.cshtml.cs` — a `PageModel` (namespace
  `FinanceApp.Areas.Identity.Pages.Account`) living in the MVC Views folder;
  duplicates `AccountController.Login`; redirects to nonexistent pages
  (`./LoginWith2fa`, `./Lockout` — `:94,99`). Dead code; delete.

**33. Phantom Identity area route + RazorPages with no pages — LOW — Phase 1**
- `Startup.cs:142-145` maps an `Identity` area route; no `Areas\` folder
  exists. `AddRazorPages`/`MapRazorPages` (`:72,140`) serve nothing.

**34. Port configuration three ways — MEDIUM — Phase 1/3**
- `Program.cs:21` — `ListenAnyIP(PORT ?? 10000)` (explicit Kestrel config)
- `appsettings.Development.json:16` / both `appsettings.Production.json` —
  `Kestrel:Endpoints:Http:Url = http://*:8888`
- `Properties\launchSettings.json:9` — `applicationUrl: http://localhost:10000`
- The explicit `UseKestrel` callback and the config section are both applied
  (config first, then the callback adds another listener) — the app likely
  binds **both** 8888 and 10000 [MEDIUM confidence — verify at first run].
  CLAUDE.md's "app binds 8888" is therefore only half the story. Standardize
  on 8888 (free per port landscape) in Phase 3.

**35. Forced `nvarchar(MAX)`/`text` on ALL string columns — HIGH — Phase 1**
- `Data\ApplicationDbContext.cs:24-41` — overrides every string column type,
  including Identity's indexed columns (`NormalizedUserName`,
  `NormalizedEmail`, maxLength 256). On SQL Server, `nvarchar(MAX)` cannot be
  an index key → creating the schema on the dev SQL Server path would fail.
  On PG, `text` is fine but the hack is unnecessary. Delete the loop when
  standardizing on PostgreSQL.

**36. Migration history is PostgreSQL-only — HIGH — Phase 1**
- The single migration `Migrations\20230411094536_FixColumnType.cs` is
  Npgsql-generated (`NpgsqlValueGenerationStrategy` annotations `:58,79,...`;
  `text`/`numeric`/`timestamp without time zone` types). It cannot apply to
  SQL Server, so the Development branch of `Startup.cs:47` has no usable
  migration history. Confirms PostgreSQL-everywhere as the only consistent
  path. Plan: regenerate a clean initial migration for net8 + Npgsql 8.

**37. Money columns lack precision/scale — MEDIUM — Phase 1**
- `numeric` (unbounded) on `Amount`, `TargetAmount`, `CurrentAmount`
  (snapshot `:105-106,120-121,144-145`). Works on PG, but set explicit
  `numeric(18,2)` per the money-handling rule when regenerating migrations.

**38. `timestamp without time zone` + `DateTime.Now` — MEDIUM — Phase 1 hazard**
- Migration uses `timestamp without time zone`; code uses local `DateTime.Now`
  (`DashboardController.cs:34`, `SeedData.cs:21,30`). Npgsql 6+ enforces
  UTC/`timestamptz` semantics — this WILL surface as runtime exceptions during
  the net8/Npgsql 8 upgrade. Plan the DateTime strategy (UTC everywhere)
  up front.

### CI / tests / repo hygiene

**39. CI pipeline is dead and was never a real pipeline — HIGH — Phase 5**
- `.github\workflows\heroku_deploy.yml` — single job: `heroku container:push`
  to app `fin-trak` (dead). **No build step, no test step, no dotnet setup.**
  `actions/checkout@v2` is deprecated. Assumes Heroku CLI on the runner.
  Complete rewrite in Phase 5 (build + test + docker build/push).

**40. Test suite is one empty test — HIGH — Phase 1 onward**
- `FinanceApp.Tests\UnitTest1.cs` — single `[Fact]` with an empty body (asserts
  nothing); an empty `Services\` folder placeholder in the csproj (`:28`).
  xUnit 2.4.1 / Test SDK 16.9.4 / net5.0. Effective coverage: 0%. Build real
  tests alongside Phase 1 fixes (money math, report classification, auth).

**41. Config keys consumed but defined nowhere — HIGH — Phase 3**
- `SendGrid:ApiKey` — read at `Startup.cs:85`
- `OpenAI:ApiKey` — read at `Startup.cs:89`, `SpendingAnalysisController.cs:59,110`
- `OpenAI:Endpoint` — read at `SpendingAnalysisController.cs:60,111`
- None exist in any appsettings file (verified all four). Email and AI features
  are configured-to-fail. Phase 3: document required env vars, fail fast at
  startup, gitignored `appsettings.Development.json` for local secrets.
  Note: `appsettings.Development.json` **is currently committed** (contains
  only a local SQL Server connection string + Kestrel 8888 — no secret leak,
  but it must come out of the repo before real secrets go in).

**42. Hardcoded values — MEDIUM — Phase 1/3**
- `Services\IEmailService\SendGridEmailService.cs:20` — sender
  `ken@backendinsight.com` / display name "FinTrak"
- `Controllers\EmailController.cs:21` — recipient `ken@backendinsight.com`
- `Data\SeedData.cs:17,26,45,55` — user GUID
- `OpenAIServices.cs:22` — base address `https://api.openai.com`
- `Helpers\CurrencyHelper.cs:12-20` — culture map

**43. SendGridEmailService: client-per-call, result ignored — LOW — Phase 1**
- `SendGridEmailService.cs:17` — `new SendGridClient(_apiKey)` per send; `:27`
  — response status never checked, failures silently swallowed.

**44. Currency model is double-booked and the formatter is wrong — MEDIUM — Phase 1**
- `Models\Transaction.cs:27,29` — both `PreferredCurrency` (string) and
  `Currency` (enum) on every transaction.
- `Helpers\CurrencyHelper.cs:18-20` — anything that isn't USD/EUR formats as
  GBP, so the enum's own `SEK` and `USH` render with a `£` sign. Wrong-currency
  display in a finance app is a correctness issue.

**45. OpenAIService retry loop retries non-transient failures — LOW — Phase 1**
- `OpenAIServices.cs:78-84` — catches `HttpRequestException` including the ones
  it throws itself for 4xx/5xx (`:75`), retrying non-transient errors; `throw ex`
  (`:82`) destroys the stack trace. Moot after the Phase 1 rewrite (item 2).

**46. Wrong logger category — LOW — Phase 1**
- `SpendingAnalysisController.cs:24,28` — injects `ILogger<AccountController>`.

**47. `Views\FinancialGoals\Create.cshtml` builds on `UpdateFinancialGoalViewModel` — LOW — Phase 1**
- `FinancialGoalsController.cs:38,44` — Create flow reuses the "Update" view
  model (naming/intent smell only).

**48. README cosmetics & structure — MEDIUM — Phase 7**
- Covered under item 11; full rewrite planned anyway.

**49. `.claude/settings.local.json` present and not ignored — LOW — now**
- `.gitignore` has no `.claude/` entry; local harness settings must not be
  committed. Addressed in the Phase 0 commit (single `.gitignore` line).

---

## 3. What is NOT broken (verified clean)

- **Money CLR types**: `decimal` everywhere, no float/double (see §1 money audit).
- **Secrets**: no API keys, passwords, or live connection strings anywhere in
  the tree. Both `HerokuConnection` values are empty/placeholder-token. ✅
- **CSRF**: all forms use tag helpers (auto antiforgery token) and all POSTs
  except item 23 validate it.
- **XSS**: no raw user input rendered; the three `@Html.Raw` uses are
  `Json.Serialize` of server-side enums/decimals.
- **SQL injection**: EF Core LINQ throughout; no raw SQL anywhere.
- **Read paths are tenant-scoped**: every Index/Report read filters on
  `UserId` (the IDOR holes are on write/delete and export paths only).

## 4. Pending decisions — recommendations

### a) AI replacement: **Anthropic API direct** (recommended) vs self-hosted Dify
**Recommendation: Anthropic API direct.**
- The feature is one prompt → one text response. A direct Messages API call
  (System.Text.Json, typed client via `IHttpClientFactory` — fixing item 5
  properly) is ~100 lines and has no infrastructure footprint.
- Dify adds a whole self-hosted service to docker-compose/k8s for a single
  endpoint — it inflates Phases 2/4 and gives the demo more ways to be down.
- Portfolio/LinkedIn angle is at least as strong: "replaced a dead OpenAI
  legacy-completions integration with Claude using structured prompting over
  real transaction data" tells a clean modernization story, and the
  economics-background framing (AI-driven spending insight) lands on the
  *quality of the analysis prompt*, not on infra.
- Design hedge: put it behind `ISpendingAnalysisService` so the provider is
  swappable; Dify can be revisited later without rework.

### b) Demo strategy: **recorded demo + screenshots** (recommended) vs cloud deployment
**Recommendation: recorded demo + screenshots in the README.**
- The app needs auth, PostgreSQL, SendGrid, and a paid AI key. A live deployment
  is a standing cost and a standing liability for an app that won't get traffic
  — and a dead demo link is precisely debt item 4 happening again.
- Screenshots/GIF + a 2–3 min recorded walkthrough never go stale, and
  docker-compose one-command local spin-up (Phase 2) is the "try it yourself"
  story for technical reviewers.
- If a live demo is later wanted, the k8s manifests (Phase 4) make a small VPS
  deployment a follow-up, not a blocker.

## 5. Phase 0 exit state

- Inventory: 49 items (17 seed items verified — 16 confirmed, 1 corrected
  (item 8: binaries absent, feature broken); 32 new findings).
- Severity profile: 3 CRITICAL security (18, 19, 20) + 1 CRITICAL process (A0),
  17 HIGH, then MEDIUM/LOW.
- No code changed. Deliverables: this report, CLAUDE.md update, git
  reconnection + commit + push.
