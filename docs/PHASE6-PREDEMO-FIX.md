# Phase 6 Pre-Demo Fix Prompt — paste into Claude Code

The new image was built and pushed to localhost:5555/financeapp:latest but the
k8s pod has NOT been restarted yet. Also, two UI bugs remain. Fix all three
things in order.

## Step 1 — Restart the k8s pod to pick up the new image
```
kubectl rollout restart deployment/financeapp -n financeapp
kubectl rollout status deployment/financeapp -n financeapp
```
Verify: curl http://localhost:8889/health/ready returns Healthy.

## Step 2 — Audit ALL auth links in the Views
The bug: some links still use hardcoded /Identity/Account/* href strings
instead of asp-controller tag helpers. Phase 3B removed Razor Pages and
MapRazorPages(), so /Identity/Account/* returns 404.

Read these files IN FULL before touching anything:
- FinanceApp/Views/Shared/_Layout.cshtml
- FinanceApp/Views/Home/Index.cshtml
- FinanceApp/Views/Account/ (all .cshtml files)

Search for ALL occurrences of:
- /Identity/Account/
- href="/Account/  (check these are correct MVC routes)
- asp-area="Identity"

The correct MVC routes are:
- Login    → asp-controller="Account" asp-action="Login"   → /Account/Login
- Register → asp-controller="Account" asp-action="Register" → /Account/Register
- Logout   → asp-controller="Account" asp-action="Logout" (POST form, already fixed in Phase 3A)
- Manage   → this was a Razor Pages area; check if AccountController has a
             Manage/Profile action — if not, remove the link entirely

Fix every /Identity/Account/* reference. Do NOT fix what is already correct.

## Step 3 — Fix the landing page images
FinanceApp/Views/Home/Index.cshtml still has the dead Unsplash URLs
(source.unsplash.com/random — that API was shut down).

WAIT — check git status first. The fix was already committed locally in
a7aeca3 but the image built before that commit was pushed to the registry.
So the view fix IS in the codebase but the running pod has the OLD image.

After Step 1's rollout restart completes, verify http://localhost:8889 shows
the feature cards (not broken images). If yes, Steps 2 and 3 may be the
same fix — just the auth links.

If the feature cards are NOT showing after the restart, the Index.cshtml
fix did not make it into the image. In that case re-read Index.cshtml,
confirm the feature cards are in the file, rebuild:
```
& "C:\Program Files\Docker\Docker\resources\bin\docker.exe" build `
  -t localhost:5555/financeapp:latest `
  -f E:\Projects\inherited\FinanceApp\Dockerfile `
  E:\Projects\inherited\FinanceApp
& "C:\Program Files\Docker\Docker\resources\bin\docker.exe" push `
  localhost:5555/financeapp:latest
kubectl rollout restart deployment/financeapp -n financeapp
kubectl rollout status deployment/financeapp -n financeapp
```
Note: use the explicit docker.exe path — the `docker` alias causes a
file-open popup in this environment.

## Step 4 — Smoke test (show all outputs)
1. http://localhost:8889 — feature cards visible, no broken images
2. Click Sign Up — must reach /Account/Register (the registration form)
3. Click Log In — must reach /Account/Login (the login form)
4. Log in as demo@fintrak.example — dashboard loads with seeded data
5. dotnet test FinanceApp.sln — still green

## Step 5 — Commit and push
Only if any code changes were made in Step 2 or 3:
`fix: correct remaining Identity route references in views`
`git push origin master`

If only the k8s rollout was needed (no code changes), no commit needed —
just report what was fixed and confirm the app is demo-ready at :8889.
