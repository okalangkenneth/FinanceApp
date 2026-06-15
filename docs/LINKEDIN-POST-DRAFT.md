# LinkedIn Post Draft — FinTrak Rehabilitation

## Option A: Lead with the security find (recommended)
~1,200 characters

---

I inherited a 3-year-old .NET finance app with a bug I still can't believe
made it to production: an unauthenticated export endpoint that returned
every user's complete transaction history to anyone who knew the URL.
No login required. Just… everyone's financial data.

That was the first thing I fixed. Then I kept going.

The app had been abandoned since 2022 — .NET 5 (end-of-life), a dead OpenAI
integration pointing at an API endpoint that no longer exists, PDF export
that crashed at runtime because the native library directory was missing,
and a Heroku deployment that had been gone for two years.

Eight phases later:

→ Security patched before a single line of framework code was touched
→ Upgraded to .NET 8 with a full minimal hosting migration
→ Dead OpenAI replaced with Anthropic Claude (claude-haiku) behind a clean
  service interface — one prompt, one response, no extra infrastructure
→ PostgreSQL everywhere, proper numeric(18,2) money columns
→ Docker + Kubernetes with split liveness/readiness probes — zero app
  restarts during a simulated database outage
→ GitHub Actions CI: build, 64 tests, Docker smoke test on every push
→ QuestPDF + ClosedXML replacing unmaintained/commercial-licensed libraries

My Economics background made this domain feel natural. A savings rate is
just a marginal propensity to save. A budget is a resource allocation
problem. The AI spending analysis is the kind of thing I'd have written
in a seminar paper — except it actually runs.

The rehabilitation methodology I used is reusable. Audit first, security
before upgrades, one phase at a time, every phase committed and pushed.

Repo + demo in comments ↓

#dotnet #csharp #kubernetes #docker #anthropic #softwareengineering #portfolio

---

## Option B: Lead with the methodology (alternative angle)
~1,100 characters

---

Most abandoned codebases don't get rehabilitated. They get rewritten or
quietly forgotten.

I've been building a structured methodology for taking inherited .NET
projects from technical debt to production quality — and I used it on a
3-year-old personal finance app this week.

Phase 0: Discovery audit. 49 debt items, three of them critical security
vulnerabilities. The worst: an anonymous endpoint serving every user's
financial history to any unauthenticated visitor.

Phase 1A: Security first, always. Critical vulnerabilities patched before
any framework upgrade work. This is non-negotiable.

Then: .NET 5 → .NET 8. Dead OpenAI integration → Anthropic Claude API.
SQL Server/PostgreSQL split → PostgreSQL everywhere. DinkToPdf (archived,
broken at runtime) → QuestPDF. Heroku (dead 2022) → Docker + Kubernetes.

The final stack: .NET 8 · ASP.NET Core MVC · PostgreSQL 16 · EF Core 8 ·
Anthropic Claude · QuestPDF · ClosedXML · Serilog · Docker · k8s ·
GitHub Actions CI with Docker smoke test.

64 tests. Zero app restarts during a database outage (split health probes).
Green CI badge. One-command local setup.

My Economics background isn't incidental — personal finance is a domain
I understand from first principles, which made the AI spending analysis
feel less like a feature and more like an obvious addition.

Repo + demo in comments ↓

#dotnet #csharp #kubernetes #anthropic #devops #portfolio #softwareengineering

---

## Comment (post this as first comment on either version)
GitHub: https://github.com/okalangkenneth/FinanceApp
Demo: [YouTube URL]

---

## Notes for final edit before posting
- Insert the actual YouTube demo URL above
- Pick Option A or B (A is more concrete and story-driven; B is better if
  you want to emphasise the methodology for hiring managers who look for
  process over output)
- Add 1-2 sentences about Stockholm / backendinsight.com if posting
  in a Swedish tech context
- Post on a Tuesday or Wednesday morning (Stockholm time) for best reach
- Tag Anthropic (@Anthropic) in the post body if you want distribution
  from their side — they occasionally reshare Claude integrations
