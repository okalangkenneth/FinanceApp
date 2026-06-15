# Phase 6 — Demo Script & Recording Guide (UPDATED)

## The story in one sentence
A Stockholm developer's household budget: 34,200 SEK salary in, rent/groceries/
transport out, two goals in progress, one completed — four months of real-looking
data analysed by Claude AI in two clicks.

## Stack for recording
Use the k8s stack at http://localhost:8889 (compose stack :8888 may be down).
The AI features require the Anthropic key in the k8s secret — confirmed working.

## Pre-recording checklist
- [ ] Verify stack: `Invoke-RestMethod http://localhost:8889/health/ready` → Healthy
- [ ] Open http://localhost:8889 in a CLEAN browser window (no dev tools, no tabs,
      no bookmarks bar — use a clean profile or guest window)
- [ ] Browser zoom 100%, window maximised
- [ ] Win+G (Game Bar) or OBS ready — record at 1080p
- [ ] Close all notifications (Slack, email, Teams)
- [ ] Hide the taskbar if possible (right-click → taskbar settings → auto-hide)
- [ ] Login credentials ready: demo@fintrak.example / Demo!Fintrak2026

---

## Recording — Scene by Scene (~5 minutes)

### Scene 1 — Landing page (0:00–0:20)
Navigate to http://localhost:8889
Show the three feature cards: 💰 Track Spending · 🎯 Set Goals · 🤖 AI Insights
Pause 3 seconds. Don't click anything yet.

### Scene 2 — Log in (0:20–0:45)
Click Log In (top right).
Email: demo@fintrak.example
Password: Demo!Fintrak2026
Click Log in. Wait for dashboard to load fully.

### Scene 3 — Dashboard (0:45–1:45) 
The dashboard now fits above the fold — all 3 sections visible without scrolling:
- Income (¤34,200) / Expenses (¤20,051) / Balance (¤14,149) summary cards
- 5 most recent transactions table
- Financial Goals — 3 progress bar cards:
    Emergency fund → blue bar ~52%
    Summer trip to Portugal → yellow bar ~29%
    New laptop → GREEN bar 100% (Completed ✓)
Pause 4–5 seconds on the goals section. The progress bars are the visual anchor.
Slow scroll down to show the spending analysis pie chart below.

### Scene 4 — Transactions (1:45–2:15)
Click Transactions in the nav.
Scroll slowly — salary, rent, groceries, streaming, transit card, dental visit.
Real-looking categorised data across 4 months. No interaction needed.

### Scene 5 — AI Spending Analysis (2:15–3:15) ← MONEY SHOT
Click "Spending Analysis" in the nav.
The page loads immediately (no button needed — analysis runs on load).
Wait for the Claude response (2–5 seconds).
The response shows category-by-category breakdown with insights.
**PAUSE 8–10 seconds minimum** — let the recruiter read it.
Scroll slowly if the response is long.
This is the feature that replaced a dead OpenAI integration.

### Scene 6 — AI Recommendations (3:15–3:55) ← SECOND MONEY SHOT
Click "AI Recommendations" in the nav.
Response shows: Priority 1 Foundation → Priority 2 Debt & Credit →
Priority 3 Budget & Goals → Priority 4 Long-term Growth → Quick Win tip.
**PAUSE 8 seconds on the opening priorities**, then slow scroll.
Two AI features, both live, both returning real Claude responses.

### Scene 7 — Reports (3:55–4:15)
Click Reports. Show the report list (8 report types).
Click "Income vs Expense" — shows ¤143,300 income vs ¤87,444 expenses total.
Quick, no interaction.

### Scene 8 — Export (4:15–4:35)
Click Export in the nav.
Show the two download cards: Excel and PDF.
Click "Download Excel" — show the file download in the browser.

### Scene 9 — End (4:35–5:00)
Click Dashboard. Let it load.
Hold on the goal progress bars for 3 seconds.
Stop recording.

---

## Screenshots to capture (save to docs/screenshots/)
Take these as SEPARATE still screenshots — cleaner than video frames:

1. **dashboard.png** — full dashboard above the fold: 3 summary cards +
   5 transactions + 3 goal progress bars all visible. This is the hero shot.
2. **ai-analysis.png** — Spending Analysis page with full Claude response visible
3. **ai-recommendations.png** — AI Recommendations with the 4 priorities showing
4. **transactions.png** — full transaction list, multiple categories visible
5. **export.png** — Export page showing the two download cards
6. **landing.png** — landing page with the 3 feature cards

Create the folder first:
```powershell
New-Item -ItemType Directory -Path 'E:\Projects\inherited\FinanceApp\docs\screenshots' -Force
```
Use Win+Shift+S (Snip & Sketch) for each screenshot.

---

## After recording
1. Trim dead time at start/end — target 4:30–5:00 total
2. Export as MP4 (H.264, 5–10 Mbps for 1080p)
3. Upload to YouTube as Unlisted
4. Copy the URL — Phase 7 README needs it
5. Add the URL to the top of docs/PHASE7-README-OUTLINE.md before running CC

## Notes for Phase 7
- The markdown `**bold**` in AI responses renders as literal asterisks in the
  `<p>` tag. Phase 7 CC should replace `<p style="white-space: pre-wrap">` with
  a Markdown-to-HTML renderer or at minimum strip the asterisks. Note this as
  a polish item but don't block the recording on it.
- The Recommendations prompt is generic ("better financial management") — Phase 7
  can optionally make it user-data-aware like the Analyze endpoint. Out of scope
  for now.
