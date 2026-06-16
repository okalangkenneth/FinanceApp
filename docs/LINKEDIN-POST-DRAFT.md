# LinkedIn Post — FinTrak Rehabilitation
# READY TO POST — Option A selected, YouTube URL inserted

---

## POST BODY (copy this exactly)

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

## FIRST COMMENT (post immediately after, as first comment)

🎥 Demo: https://youtu.be/MA0fJQzMLPc
💻 GitHub: https://github.com/okalangkenneth/FinanceApp

---

## POSTING CHECKLIST
- [ ] Post on Tuesday or Wednesday morning Stockholm time (08:00–09:00 CET)
- [ ] Attach docs/screenshots/dashboard.png as the post image
- [ ] Post the body text above
- [ ] Immediately add the first comment with the two links
- [ ] Optional: tag @Anthropic in the body for potential reshare
- [ ] Optional: add "Based in Stockholm" before the hashtags for local reach

---

## NOTES
- YouTube URL confirmed: https://youtu.be/MA0fJQzMLPc
- GitHub: https://github.com/okalangkenneth/FinanceApp
- LinkedIn character limit is ~3,000; this post is ~1,200 — well within limit
- The "zero app restarts during a database outage" line is the most unusual
  technical detail — if engagement is low in first hour, that line alone
  is worth a follow-up comment expanding on the probe split design
