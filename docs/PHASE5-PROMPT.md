# Phase 5 Prompt — paste into Claude Code at E:\Projects\inherited\FinanceApp

Phase 4 complete (7b0e462). Read CLAUDE.md first. Phase 5 = CI/CD rewrite.
The dead Heroku workflow was already deleted (8055ad2). Goal: a real
build/test/push pipeline on GitHub Actions that a recruiter can read and
understand in 60 seconds.

## Scope decision (read before writing anything)
This is a LOCAL-registry portfolio project — there is no cloud deployment
(Phase 6 = recorded demo, not live deploy). So the pipeline does NOT need
to push to DockerHub, GHCR, ACR, or any cloud registry. What it MUST do:
1. Build and test on every push to master and every PR
2. Build the Docker image and verify it starts healthy (docker compose up
   smoke-test in CI — proves the Dockerfile is always valid)
3. Be honest: skip the k8s deploy step with a clear comment explaining that
   k8s targets a local registry and local cluster; a real cloud deploy would
   use OIDC + ACR/AKS (note this as future scope in a comment, not TODO)

## Step 1 — Audit the existing workflow skeleton
Read .github/workflows/ — only the dead heroku_deploy.yml was deleted;
check if anything else was left. Read the csproj for the exact
TargetFramework and SDK version needed. Read Dockerfile to confirm the
build stages. Do this BEFORE writing any YAML.

## Step 2 — Single workflow file: .github/workflows/ci.yml
Trigger: push to master, pull_request to master
Jobs (in order):

### job: build-and-test
- runs-on: ubuntu-latest
- steps:
  1. actions/checkout@v4
  2. actions/setup-dotnet — pin to the EXACT SDK version from csproj
     (verify against actions/setup-dotnet docs, don't guess version string)
  3. dotnet restore
  4. dotnet build --no-restore --configuration Release
  5. dotnet test --no-build --configuration Release --logger "trx;LogFileName=results.trx"
  6. actions/upload-artifact — upload the .trx results (makes test output
     visible in the Actions UI; useful for portfolio reviewers)

### job: docker-smoke-test
- runs-on: ubuntu-latest
- needs: build-and-test
- steps:
  1. actions/checkout@v4
  2. Copy .env.example → .env (sed or echo to fill placeholder values with
     CI-safe dummies — Anthropic key = dummy string, SendGrid key = empty,
     seed password = CiDemo1!; the app must START without real keys)
  3. docker compose up -d --wait (--wait uses healthchecks; verify this flag
     exists in the compose version available on ubuntu-latest before using it;
     fallback = up -d + sleep + curl if --wait isn't available)
  4. curl --fail --retry 5 --retry-delay 3 http://localhost:8889/health/ready
  5. docker compose logs app (always run, even on failure — captured as output)
  6. docker compose down

## Step 3 — Secrets in GitHub Actions
The smoke test needs NO real secrets (dummies suffice). Document this
explicitly in a comment in the workflow: "No real API keys are needed to
run the smoke test — the app starts and passes /health/ready with dummy
values because Anthropic and SendGrid calls are lazy (only on feature use)."
Do NOT add any repository secrets — this avoids confusion about what the
workflow needs to run.

## Step 4 — Verify locally before pushing
Use `act` if available (check with `act --version`); if not, push to a
branch first and watch the Actions run before merging to master. Either way,
the workflow must ACTUALLY PASS before the Phase 5 commit goes to master.
If act is not available, push to a branch `phase5-ci`, confirm green, then
merge (fast-forward) to master.

## Step 5 — Wrap-up
1. Add a CI badge to README.md:
   `![CI](https://github.com/okalangkenneth/FinanceApp/actions/workflows/ci.yml/badge.svg)`
   Place it at the top of the existing README (below the title). Do NOT
   rewrite the README — that is Phase 7. One line only.
2. Update CLAUDE.md Build Progress: Phase 5 complete; note that k8s deploy
   was intentionally omitted from CI with the reason.
3. Conventional commit: `feat: GitHub Actions CI pipeline with Docker smoke test (Phase 5)`
4. `git push origin master` — and confirm the Actions run goes green.
   If it fails, fix it before calling Phase 5 done. Show the final run URL.
