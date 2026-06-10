# Phase 1 Prompt — paste into Claude Code at E:\Projects\inherited\FinanceApp

Phase 0 is complete (docs/PHASE0-AUDIT.md, 49 items). Decisions made:
AI = Anthropic API direct behind ISpendingAnalysisService. Demo = recorded +
screenshots (no live deployment). Execute Phase 1 in the sub-phases below,
IN ORDER, each ending with a conventional commit + `git push origin master`.
Read CLAUDE.md and docs/PHASE0-AUDIT.md before touching anything.

## 1A — Security & correctness patch (FIRST, before any upgrade work)
These ship against net5.0 as-is; do not mix with framework changes.
1. ExportController: add [Authorize]; filter every export query to the current
   user (UserManager.GetUserId(User)); never expose other users' rows
2. TransactionsController: ownership checks on Edit/Delete (load by id AND
   UserId); Edit POST must not accept/reassign UserId — bind explicitly or use
   a ViewModel without UserId
3. FinancialGoalsController: same ownership pattern on Delete and
   UpdateFinancialGoal; add [ValidateAntiForgeryToken] where missing
4. Fix report income/expense classification: classify by TransactionType
   everywhere (Reports currently use Category — see audit) so Dashboard and
   Reports agree
5. Replace the single empty [Fact] with real tests covering: ownership
   enforcement on the three controllers + the report classification fix
Commit: `fix: patch IDOR/auth holes and report misclassification (Phase 1A)`

## 1B — net8.0 upgrade + dependency cleanup
1. TargetFramework net8.0; merge Startup.cs into Program.cs minimal hosting
2. Remove packages: Microsoft.AspNetCore.Identity 2.2.0, System.Net.Http,
   Npgsql...PostgreSQL.Design, Microsoft.VisualStudio.Azure.Containers.Tools;
   align all Microsoft.*/Npgsql packages to latest 8.x; remove the
   wkhtmltox Content/None csproj entries (directory doesn't exist)
3. Remove DockerDefaultTargetOS=Windows; rewrite Dockerfile for net8.0
   multi-stage (aspnet:8.0 / sdk:8.0)
4. Delete: heroku.yml, Pocfile, root-level duplicate appsettings.Production.json
5. Migrate any Newtonsoft.Json usage to System.Text.Json
6. Fix the ApplicationUser.EmailConfirmed shadow property (audit root-cause):
   remove the shadowing property, use IdentityUser.EmailConfirmed, delete the
   ConfirmEmail workaround, and collapse the contradictory Identity config to
   one intentional RequireConfirmedAccount value (true, since email flow exists)
7. Build must succeed: `dotnet build FinanceApp.sln` clean, then run tests
Commit: `feat: upgrade to .NET 8, purge dead dependencies (Phase 1B)`

## 1C — AI service rewrite (Anthropic Messages API)
1. Define ISpendingAnalysisService (AnalyzeSpendingHabitsAsync,
   GenerateRecommendationsAsync) and register via AddHttpClient — typed client,
   NO singleton-capturing-HttpClient pattern (audit DI bug)
2. Implement AnthropicSpendingAnalysisService. ANTI-HALLUCINATION: verify the
   current official Anthropic .NET SDK status via docs (docs.claude.com /
   NuGet) before choosing SDK vs raw typed HttpClient against
   POST https://api.anthropic.com/v1/messages. Do not write the API shape
   from memory.
3. Model: claude-haiku-4-5-20251001 (matches the rest of the portfolio's
   cost profile). Config key Anthropic:ApiKey from appsettings.Development.json
   (gitignored) — verify .gitignore covers it BEFORE writing the file; never
   commit a key. Production reads from environment variable.
4. Delete OpenAIServices.cs, CreditExhaustedException if OpenAI-specific,
   HttpRequestMessageExtensions if now unused (audit flagged
   CloneHttpRequestMessage as dead). Remove OpenAI config references.
5. Keep retry/backoff behavior but use Polly or HttpClientFactory resilience
   (Microsoft.Extensions.Http.Resilience) instead of the hand-rolled loop
Commit: `feat: replace dead OpenAI integration with Anthropic Messages API (Phase 1C)`

## 1D — PostgreSQL everywhere + library swaps
1. Remove UseSqlServer branch and the SqlServer EF package; single UseNpgsql
   path; dev connection string targets localhost:5434 (FinanceApp's dedicated
   container arrives in Phase 2; 5433 is occupied by another project)
2. Verify the existing Npgsql migration applies cleanly against net8/EF8;
   regenerate if the model changed in 1B (EmailConfirmed removal WILL change
   the model — create a follow-up migration, do not edit the old one)
3. PDF: replace DinkToPdf with QuestPDF; set QuestPDF.Settings.License =
   LicenseType.Community and note the license terms in a code comment
4. Excel: replace EPPlus with ClosedXML in ExportController
5. Run full build + tests; smoke-test export endpoints compile paths
Commit: `feat: PostgreSQL-only data layer, QuestPDF + ClosedXML exports (Phase 1D)`

## 1E — History purge (STOP POINT — requires manual action)
1. STOP and tell Kenneth to temporarily disable the locked-branch ruleset on
   master (GitHub → Settings → Rules) before proceeding. Do not continue until
   he confirms.
2. After confirmation: remove FinanceApp.git/ via git-filter-repo (verify
   install: `git filter-repo --version`), purge it from ALL history, re-add
   origin remote (filter-repo strips it), force-push master
3. Remind Kenneth to re-enable the ruleset, then verify the GitHub language
   stats no longer show Shell/Perl inflation
4. Update CLAUDE.md Build Progress (Phase 1 → COMPLETED, list debts resolved
   vs deferred) before the final push
Commit: `chore: purge committed bare repo from history (Phase 1E)`
