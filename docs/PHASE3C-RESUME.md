# Phase 3C RESUME Prompt — paste into Claude Code at E:\Projects\inherited\FinanceApp

The previous session hit the usage limit mid-Phase 3C. Exact known state:
- 3A (29cab6b) and 3B (e4b5283) are COMMITTED LOCALLY, NOT pushed
- 3C is partially done and UNCOMMITTED. Modified: SeedData.cs,
  FinanceApp.csproj, CurrencyHelper.cs, FinancialGoal.cs, Transaction.cs,
  SendGridEmailService.cs; new untracked: FinanceApp/Configuration/
- Program.cs is UNTOUCHED — so 3C items 4 (port standardization) and
  8 (Serilog) plus options registration in DI are almost certainly not done
- Nothing in 3C has been built or tested yet this round

## Step 1 — Reconstruct, don't assume
Read docs/PHASE3-PROMPT.md section 3C (the 8-item checklist), then:
`git diff` + `git status` and read every modified/new file IN FULL.
Produce a short checklist: for each 3C item 1-8, state DONE / PARTIAL /
NOT STARTED with evidence from the diff. Do not rewrite work that's done.

## Step 2 — Complete 3C
Finish only the gaps from your checklist. Expected remaining at minimum:
- Register the options classes in Program.cs (binding + validation)
- Item 4: port standardization in Program.cs (re-test container boot after —
  interacts with Phase 2 decision (d), see CLAUDE.md)
- Item 8: Serilog (console, structured, UseSerilogRequestLogging)
- Any unit tests the checklist shows missing (Progress zero-guard, SEK/USH
  currency formatting)

## Step 3 — Wrap-up (from PHASE3-PROMPT.md, unchanged)
1. Full stack re-test: docker compose up -d --build; /health; register +
   login + SEED_DEMO_DATA visible on dashboard; AI endpoint 429s when
   hammered; dotnet test green. Show outputs.
2. Commit 3C: `feat: options pattern, Serilog, currency + seeding fixes (Phase 3C)`
3. Update CLAUDE.md Build Progress: record Phase 3 complete, mark audit items
   17,21,22,24,25,28,29,30,31,32,33,34,42,43,44,46,47 resolved, note deferrals.
   Also record THIS interruption pattern: phase work must be pushed at every
   sub-phase commit, not only at phase end (see new rule below).
4. Push: `git push origin master` — this publishes 3A + 3B + 3C together.
