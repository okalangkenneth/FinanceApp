# Phase 7 Prompt — paste into Claude Code at E:\Projects\inherited\FinanceApp

Phase 6 complete. Stack running at :8889 (k8s, image p7final). Read CLAUDE.md
and docs/PHASE7-README-OUTLINE.md before starting. Phase 7 = markdown rendering
fix + README rewrite + landing page polish + final push.

**DEMO VIDEO URL**: [INSERT YOUTUBE URL HERE before running this prompt]
If not yet recorded, substitute a placeholder and note it in the README as
"Demo video coming soon" — Kenneth will update after recording.

## Step 1 — Fix markdown rendering in AI responses (small code change)
The Anthropic API returns markdown in its responses (**bold**, ## headings,
- bullets). The current views render it as raw text, so **bold** shows as
literal asterisks.

Fix in both views:
- FinanceApp/Views/SpendingAnalysis/Analyze.cshtml
- FinanceApp/Views/SpendingAnalysis/Recommendations.cshtml

Approach: use the Markdig NuGet package (MIT licence) to convert the markdown
response to HTML before rendering. Steps:
1. Add Markdig to FinanceApp.csproj — verify latest stable version on NuGet
   before adding, do not guess the version number
2. In each view, pass the AI response through Markdig.Markdown.ToHtml() and
   render with @Html.Raw() instead of a plain string
3. Add a small CSS rule to site.css scoping the rendered markdown div:
   .ai-response h2, h3 { margin-top: 1rem; }
   .ai-response ul { padding-left: 1.5rem; }
   .ai-response strong { color: inherit; }
   Wrap the rendered HTML in <div class="ai-response">
4. Build and verify: dotnet build must be clean; run the app locally with
   dotnet run and confirm the AI response renders with proper bold/headings

DO NOT use JavaScript markdown renderers (marked.js etc.) — keep it server-side.
Commit: `feat: render AI responses as markdown via Markdig (Phase 7)`

## Step 2 — README rewrite
Follow docs/PHASE7-README-OUTLINE.md exactly. Key requirements:
- Natural voice, not marketing copy
- The anonymous export vulnerability is the opening hook
- The Before/After rehabilitation table is the centrepiece
- No false claims (no SignalR, no Heroku, no OpenAI, no SQL Server)
- Screenshots from docs/screenshots/ (relative paths) — use whatever files
  exist there; if a screenshot file is missing, use a placeholder comment
  <!-- screenshot: dashboard.png --> so Kenneth knows where to drop it
- Demo video: embed if YouTube URL provided above, otherwise placeholder
- CI badge already in README — keep it at the top
- © 2023 footer was already fixed to 2026 in the app; README has no footer
  to update but remove any "2023" date references in the README body
- DO NOT rewrite the "Run locally" section added in Phase 2 — it is accurate;
  just clean up formatting if needed

Commit: `docs: rewrite README — rehabilitation story, honest feature list (Phase 7)`

## Step 3 — Final polish
1. Check every nav link still works after any view changes in Step 1:
   Dashboard, Transactions, FinancialGoals, Reports, Spending Analysis,
   AI Recommendations, Export, Logout
2. Run dotnet test — must be 64/64 green
3. Rebuild and push the final image (use explicit docker path — the `docker`
   alias causes a file-open popup in this environment):
   & "C:\Program Files\Docker\Docker\resources\bin\docker.exe" build --no-cache `
     -t localhost:5555/financeapp:phase7 `
     -f E:\Projects\inherited\FinanceApp\Dockerfile `
     E:\Projects\inherited\FinanceApp
   & "C:\Program Files\Docker\Docker\resources\bin\docker.exe" push `
     localhost:5555/financeapp:phase7
   kubectl set image deployment/financeapp financeapp=localhost:5555/financeapp:phase7 `
     -n financeapp
   kubectl rollout status deployment/financeapp -n financeapp
4. Verify by CONTENT (not just status code):
   (Invoke-RestMethod 'http://localhost:8889/') -match 'Track Spending' → True
   Visit /SpendingAnalysis/Recommendations — confirm bold text renders as
   <strong>, not **asterisks**
5. Update k8s/05-app.yaml image tag to phase7
6. Update CLAUDE.md Build Progress: Phase 7 complete; note demo video URL
   (or placeholder); note any items deferred to future work

## Step 4 — Final commits and push
All commits in this phase must be pushed immediately after each commit.
Final sequence:
- Step 1 commit (Markdig)
- Step 2 commit (README)  
- Step 3 commit (k8s manifest + CLAUDE.md update)
- `git push origin master` after EACH commit

Confirm final state:
- GitHub repo shows clean master with Phase 7 as the latest commit
- CI badge on README is green (the push triggers the Actions run)
- /health/ready → Healthy
- AI responses render markdown properly
- README tells the rehabilitation story honestly

Phase 7 complete = project done.
