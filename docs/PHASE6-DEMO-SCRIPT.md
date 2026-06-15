# Phase 6 — Demo Script & Recording Guide

## The story in one sentence
A Stockholm developer's household budget: 34,200 SEK salary in, rent/groceries/
transport out, two goals in progress, one completed — four months of real-looking
data analysed by AI in one click.

## Pre-recording checklist
- [ ] `docker compose down` (stop the k8s stack if running on :8889)
- [ ] Confirm .env has SEED_DEMO_DATA=true and a real SeedDemo:Password value
- [ ] `docker compose up -d` and wait for /health/ready (30–60s)
- [ ] Open http://localhost:8888 in a clean browser window (no dev tools, no tabs)
- [ ] Set browser zoom to 100%, window maximised or 1280×800
- [ ] OBS or Win+G ready; record 1080p or 1280×800 minimum
- [ ] Close Slack/email notifications

---

## Recording — Scene by Scene (~4 minutes total)

### Scene 1 — Landing page (0:00–0:20)
Show http://localhost:8888 briefly.
**Do not narrate** — the recording is silent; text overlays go in the edit.

### Scene 2 — Log in (0:20–0:40)
Click Log In.
Email: demo@fintrak.example
Password: (your .env value)
Submit. Let the dashboard load.

### Scene 3 — Dashboard (0:40–1:30)
Slow scroll. The recruiter should see:
- Total income / expenses / net this month
- The goal progress cards:
  Emergency fund → 52.8% (47,500 / 90,000 SEK)
  Portugal trip  → 28.9% (5,200 / 18,000 SEK)
  New laptop     → COMPLETED ✓
- The monthly transaction chart showing 4 months of activity
Pause 3–4 seconds on each card. Don't rush.

### Scene 4 — Transactions (1:30–2:00)
Click Transactions in the nav.
Scroll the list — shows salary, rent, groceries, the concert ticket splurge,
the freelance income, dental visit. Real-looking, categorised data.
No interaction needed — just show the breadth.

### Scene 5 — AI Spending Analysis (2:00–3:00) ← MONEY SHOT
Click Spending Analysis (or the nav link).
Click Analyse / Generate.
Wait for the Anthropic response (~2–4s).
Let the recommendation text display fully on screen.
Pause 5 seconds minimum — this is the feature. Let it breathe.

### Scene 6 — Reports (3:00–3:20)
Click Reports. Show the income vs expense summary,
the category breakdown. Quick scroll, no interaction.

### Scene 7 — Export (3:20–3:40)
Click Export → Export to Excel (or PDF).
Show the file download. You don't need to open it.

### Scene 8 — End (3:40–4:00)
Return to dashboard. Hold 3 seconds. Stop recording.

---

## Screenshots to capture (for README, separate from recording)
Take these as still screenshots before or after the recording:

1. **dashboard.png** — dashboard in full, goals visible, chart visible
2. **transactions.png** — transaction list, category icons visible
3. **ai-analysis.png** — spending analysis with the AI response showing
4. **goals.png** — goals page, all three goals (2 in progress + 1 completed)
5. **reports.png** — monthly report view
6. **ci-badge.png** — skip, the badge is in the README already

Save all screenshots to docs/screenshots/ — Phase 7 references them from there.

---

## After recording
1. Trim the video (remove dead time at start/end); keep it under 4 minutes
2. Export as MP4 (H.264, reasonable bitrate — 5–10 Mbps for 1080p)
3. Upload to YouTube (Unlisted is fine) OR keep as a file and link from README
4. Note the URL — Phase 7 needs it for the Demonstration section
