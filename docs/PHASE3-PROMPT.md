# Phase 3 Prompt — paste into Claude Code at E:\Projects\inherited\FinanceApp

Phase 2 complete (stack runs at :8888). Read CLAUDE.md + docs/PHASE0-AUDIT.md.
Phase 3 = service wiring & configuration hygiene: close every remaining
security item, delete the dead/broken surface, and make configuration
first-class. Audit item numbers below refer to PHASE0-AUDIT.md.

## 3A — Remaining security items (one commit)
1. Item 21: DELETE EmailController entirely (anonymous SendGrid send,
   hardcoded recipient, missing view). Remove its routes/links if any.
2. Item 22: [Authorize] on DashboardController and SpendingAnalysisController —
   anonymous users must not trigger paid Anthropic calls. ALSO add the net8
   built-in rate limiter (AddRateLimiter) with a tight policy on the
   spending-analysis endpoint only (e.g. a few requests/minute per user) —
   it's a metered API behind a button; verify exact middleware API against
   MS docs, don't write from memory.
3. Item 24: replace "User not found." with the same generic invalid-login
   message for both cases; enable lockoutOnFailure: true.
4. Item 25: logout must be POST + [ValidateAntiForgeryToken]; update the
   layout link to a form. Delete HomeController.LogOutAndRedirect.
Commit: `fix: close remaining auth/abuse surface (Phase 3A)`

## 3B — Dead and broken surface removal (one commit)
1. Item 28: delete HomeController.Dashboard (broken duplicate; view missing,
   totals from last 5 rows). Keep DashboardController as the only dashboard.
2. Item 30: AccountController.Profile — view is missing (500). Decide by
   inspecting what it returns: if trivial, create the view; if redundant
   with Identity manage pages, delete the action. State your reasoning.
3. Item 31: delete orphan Views/FinancialGoals/Edit.cshtml (posts to
   nonexistent action) unless you wire a real Edit; prefer delete — Create/
   Update flows already exist.
4. Item 32: delete the misplaced Razor Pages LoginModel
   (Views/Shared/Login.cshtml.cs) and its duplicate login flow.
5. Item 33: remove the phantom Identity area route; remove AddRazorPages/
   MapRazorPages if genuinely no pages remain after item 32 — verify first.
6. Item 46: fix ILogger<AccountController> in SpendingAnalysisController →
   ILogger<SpendingAnalysisController>. Item 47: rename the Create flow
   viewmodel usage (CreateFinancialGoalViewModel) — small, do it here.
7. Build + run full test suite after deletions; add a smoke test that all
   remaining controller actions have views (or return non-View results).
Commit: `refactor: remove dead controllers, views, duplicate flows (Phase 3B)`

## 3C — Configuration first-class + correctness leftovers (one commit)
1. Options pattern: introduce AnthropicOptions and SendGridOptions bound from
   configuration sections with validation on start
   (ValidateDataAnnotations/ValidateOnStart where it makes sense — SendGrid
   stays optional-with-warning per Phase 2 decision; Anthropic key required
   only when the feature endpoint is hit, not at boot — implement gracefully).
2. Item 43 full hardening: SendGridEmailService becomes a properly injected
   typed/singleton client; CHECK the send response status and log failures
   with context; no per-call client construction.
3. Item 42: kill hardcoded ken@backendinsight.com sender/recipient → config
   (Email:Sender etc. in options); kill the hardcoded seed GUID alongside
   item 17 below.
4. Item 34: standardize ports. Program.cs drops ListenAnyIP(PORT??10000);
   single source of truth = ASPNETCORE_HTTP_PORTS-style config; host dev runs
   on 8888 via launchSettings; compose keeps 8888:8080. Verify the container
   still boots after the change (this interacts with Phase 2 decision (d) —
   re-test, update the compose env if the PORT var is now unnecessary).
5. Item 29: FinancialGoal.Progress — guard TargetAmount == 0 (return 0).
   Add a unit test for it.
6. Item 44: remove the Transaction PreferredCurrency/Currency duplication
   (keep the enum); fix CurrencyHelper so SEK/USH/USD/GBP format with the
   correct symbols — use CultureInfo-based formatting rather than a
   hand-rolled symbol map where possible. Unit tests for SEK and USH.
7. Item 17: SeedData decision — implement proper dev seeding behind an env
   flag (SEED_DEMO_DATA=true in compose only): one demo user with a config-
   sourced password (never hardcoded), a few months of realistic transactions
   and 2-3 goals. This is also what the Phase 6 recorded demo will showcase —
   make the data tell a story (salary in, rent/groceries out, a savings goal
   mid-progress).
8. Serilog: replace default logging with Serilog (console sink, structured)
   per portfolio convention — keep it lean, no ELK here; request logging via
   UseSerilogRequestLogging.
Commit: `feat: options pattern, Serilog, currency + seeding fixes (Phase 3C)`

## Wrap-up
1. Full stack re-test: docker compose up -d --build, /health Healthy,
   register + login + seed data visible on dashboard, AI endpoint returns 429
   when hammered (rate limiter proof), dotnet test all green. Show outputs.
2. Update CLAUDE.md: Build Progress + mark audit items 17,21,22,24,25,28-34,
   42,43,44,46,47 resolved; note anything deliberately deferred.
3. `git push origin master`
