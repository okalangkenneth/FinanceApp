# Phase 7 — README Rewrite Outline

This is the OUTLINE for CC to follow in Phase 7. Do NOT write the README yet.
The demo video URL (from Phase 6) must be inserted before CC runs.

---

## README structure (replace the entire current README)

### Title + badges
```
# FinTrak — Personal Finance Manager
[![CI](badge-url)] [![.NET](https://img.shields.io/badge/.NET-8.0-purple)]
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow)]
```

### One-paragraph hook (lead with the problem found, not the tech)
Something like:
"Inherited a 3-year-old .NET 5 finance app with an anonymous data-export
endpoint that served every user's transaction history to any unauthenticated
visitor. Patched the vulnerability, upgraded the stack to .NET 8, replaced
a dead OpenAI integration with the Anthropic Claude API, containerised with
Docker and Kubernetes — and rehabilitated it into a production-quality
portfolio piece. The Economics background isn't accidental: personal finance
is a domain I understand from first principles."
(CC rewrites this in its own natural voice — this is the intent, not the copy.)

### Demo
- Embedded YouTube video (or link if embedding doesn't render on GitHub)
- 3–4 screenshots inline: dashboard, AI analysis, goals, transactions
  (from docs/screenshots/ — CC uses relative paths: ![Dashboard](docs/screenshots/dashboard.png))

### What was fixed (the rehabilitation story — this IS the portfolio signal)
A table or short list:
| Before | After |
|--------|-------|
| Anonymous export — all users' data to anyone | [Authorize] + per-user filtering |
| .NET 5 (EOL 2022) | .NET 8 LTS |
| Dead OpenAI completions API | Anthropic Claude (claude-haiku) |
| SQL Server dev / PostgreSQL prod split | PostgreSQL everywhere |
| DinkToPdf (archived, broken) | QuestPDF (Community) |
| EPPlus (commercial licence) | ClosedXML (MIT) |
| Heroku deploy (dead since 2022) | Docker + Kubernetes + GitHub Actions CI |

### Features (honest — no SignalR claim, no dead Heroku link)
- Dashboard: income / expense / net, monthly chart, goal progress cards
- Transaction management with category classification
- Financial goals with progress tracking
- AI spending analysis (Anthropic Claude API)
- PDF and Excel export
- Email confirmation (SendGrid)
- Multi-currency support (SEK default)
- Reports: income vs expense by category

### Tech stack
.NET 8 · ASP.NET Core MVC · EF Core 8 · PostgreSQL 16 · ASP.NET Core Identity
Anthropic Claude API · QuestPDF · ClosedXML · Serilog · SendGrid
Docker · docker-compose · Kubernetes (manifests in k8s/) · GitHub Actions CI

### Run locally
(Keep the Phase 2 section — already accurate, just clean it up)
```bash
cp .env.example .env   # add your Anthropic API key; app runs without it
docker compose up
```
http://localhost:8888 · Demo user: demo@fintrak.example (set SEED_DEMO_DATA=true)

### Security notes
Short paragraph: the three critical vulnerabilities found and patched,
the principle of security-first before any upgrade work.

### Licence
MIT

---

## Phase 7 instructions for CC
1. Insert the Phase 6 demo video URL before running (Kenneth provides it)
2. Rewrite README.md following this outline — natural voice, not marketing copy
3. Update footer year: © 2023 → © 2026
4. Fix the three broken landing page images (Phase 4 backlog item):
   - Check Views/Home/Index.cshtml for the <img> src attributes
   - Either provide real images in wwwroot/images/ or remove the broken tags
   - Do not use placeholder.com URLs — either real assets or clean removal
5. dotnet test — still 64/64 green
6. docker compose up -d --build; verify :8888 loads and demo user works
7. Conventional commit: `docs: rewrite README, fix landing page images (Phase 7)`
8. `git push origin master`
9. Paste the final GitHub repo URL here — Phase 7 complete, project done
