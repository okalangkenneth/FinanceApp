# Phase 0 Prompt — paste into Claude Code at E:\Projects\inherited\FinanceApp

You are starting Phase 0 (Discovery Audit) of an inherited project rehabilitation.
Read CLAUDE.md first — it contains a 17-item seed debt list from pre-audit
reconnaissance. Your job is to verify, correct, and complete that inventory.
DO NOT change any code in this phase. Read-only, plus writing the audit report.

## Step 1 — Full structural read
Read every source file. Known structure:
- FinanceApp/Controllers/, Models/, Services/, Data/, Helpers/, Exceptions/,
  Migrations/, Views/, wwwroot/
- FinanceApp.Tests/
- Root: Dockerfile, heroku.yml, Pocfile, .github/workflows/, FinanceApp.git/
Use batched reads. Skip wwwroot/lib vendor files and FinanceApp.git/ contents
(just confirm it is a bare repo: check for HEAD, config, objects/).

## Step 2 — Verify the seed list
For each of items 1–17 in CLAUDE.md, confirm or refute with file path + line
evidence. Pay specific attention to:
- Item 11: search all Views/ and wwwroot/js for SignalR and Chart.js usage;
  compare against README feature claims, list every claim not backed by code
- Item 16: grep for usages of CloneHttpRequestMessage
- Money handling: read Models/Transaction*.cs and Goal models — report the CLR
  type used for amounts (decimal vs double) and any EF column type config

## Step 3 — Extend the inventory
Audit for debt the seed list missed. Minimum checklist:
- .github/workflows/: what does CI actually do, what is broken
- FinanceApp.Tests/: test count, what they cover, do they target net5.0
- Migrations/: SQL Server vs PostgreSQL migration history state
- Controllers: auth attributes, input validation, over-posting risks
- appsettings*: every key consumed via Configuration[...] vs what actually exists
  (note: SendGrid:ApiKey and OpenAI:ApiKey are read in Startup but defined nowhere)
- Views: XSS surface (raw Html usage), CSRF tokens on forms
- Helpers/, Exceptions/, HttpRequestMessageExtensions/: dead code candidates
- Hardcoded URLs, connection strings, magic numbers

## Step 4 — Deliverables (then STOP — no Phase 1 work)
1. Write docs/PHASE0-AUDIT.md: numbered, complete debt inventory; each item with
   severity (CRITICAL/HIGH/MEDIUM/LOW), file:line evidence, proposed phase
2. Update CLAUDE.md: replace the seed list with the verified inventory; update
   Build Progress (Phase 0 → COMPLETED)
3. Report the two pending decisions with your recommendation + rationale:
   a) AI replacement: Anthropic API direct vs self-hosted Dify workflow
   b) Demo strategy: recorded demo + screenshots vs small cloud deployment
4. Commit: `git add -A && git commit -m "docs: Phase 0 discovery audit"` then
   `git push origin master`
