# K8s Restore Script Prompt — paste into Claude Code

Write a PowerShell script at scripts/k8s-restore.ps1 that fully restores
the FinTrak k8s stack after a Docker Desktop Kubernetes reset. The script
must be runnable with a single command and require zero copy-pasting.

## Context
- Cluster: Docker Desktop (context: docker-desktop)
- Namespace: financeapp
- Local registry: localhost:5555 (Docker container named "registry")
- Current app image: localhost:5555/financeapp:phase7
- Secrets are ALWAYS wiped by a cluster reset — must be recreated from .env
- NEVER apply k8s/03-secret.example.yaml — it contains literal placeholder
  strings that corrupt the secret and break migrations
- The docker alias causes a file-open popup — always use the explicit path:
  C:\Program Files\Docker\Docker\resources\bin\docker.exe

## Script requirements

### 0. Prerequisites check (fail fast with clear message)
- kubectl context is docker-desktop → error and exit if not
- Node desktop-control-plane is Ready → wait up to 30s, error if not
- Registry is answering on :5555 → if not, start it:
  & "C:\Program Files\Docker\Docker\resources\bin\docker.exe" start registry
  Wait 5s, retry once, error if still down

### 1. Read secrets from .env
Parse E:\Projects\inherited\FinanceApp\.env for:
- ANTHROPIC_API_KEY (required — error and exit if missing or empty)
- SEED_DEMO_PASSWORD (optional — warn if missing, use default Demo!Fintrak2026)
- SENDGRID_API_KEY (optional — empty string if missing, app handles gracefully)
- SENDGRID_SENDER_EMAIL (optional — empty string if missing)

Parse robustly: strip comments (lines starting with #), handle blank lines,
handle the case where value is on the same line as key (KEY=value) AND the
broken format where value was on the next line (KEY=\nvalue) — normalize both.

### 2. Deploy in correct order with status output
Print progress with timestamps. Each step:

Step 1: Apply namespace and configmap
  kubectl apply -f k8s/00-namespace.yaml
  kubectl apply -f k8s/01-configmap.yaml

Step 2: Create/update secret from parsed .env values
  kubectl -n financeapp create secret generic financeapp-secrets \
    --from-literal=ConnectionStrings__DefaultConnection=\
      "Host=postgres;Port=5432;Database=financeapp;Username=financeapp;Password=financeapp_dev_pw" \
    --from-literal=Anthropic__ApiKey="$anthropicKey" \
    --from-literal=SeedDemo__Password="$seedPassword" \
    --from-literal=SendGrid__ApiKey="$sendGridKey" \
    --from-literal=SendGrid__SenderEmail="$sendGridEmail" \
    --save-config --dry-run=client -o yaml | kubectl apply -f -
  (dry-run + apply pattern updates if exists, creates if not — no delete needed)

Step 3: Apply postgres StatefulSet
  kubectl apply -f k8s/02-postgres.yaml
  Wait for postgres ready: poll kubectl get pod postgres-0 -n financeapp
  every 5s up to 90s. Print dots while waiting. Error with pod logs if timeout.

Step 4: Run migration job
  Delete stale job if present (ignore-not-found):
    kubectl delete job financeapp-migrate -n financeapp --ignore-not-found
  Apply: kubectl apply -f k8s/04-migration-job.yaml
  Wait for completion: poll every 5s up to 120s.
  On timeout: print kubectl logs job/financeapp-migrate -n financeapp
  and exit with error.

Step 5: Deploy app
  kubectl apply -f k8s/05-app.yaml
  kubectl rollout status deployment/financeapp -n financeapp --timeout=120s
  On failure: print pod logs and exit with error.

### 3. Verify by content (not just status)
After rollout:
- /health/live → must return "Healthy"
- /health/ready → must return "Healthy"  
- / content → must contain "Track Spending" (proves new image, not cached old)
- /Account/Login → must return HTTP 200 (auth routes working)
Print PASS/FAIL for each check.

### 4. Final summary
Print a clear summary box:
  ✅ FinTrak k8s stack restored successfully
  App:      http://localhost:8889
  Demo user: demo@fintrak.example / [SEED_DEMO_PASSWORD from .env]
  Health:   http://localhost:8889/health/ready
  Time:     Xm Xs

### Error handling
- Every kubectl command checks exit code; on failure print the command that
  failed, relevant pod/job logs, and exit with code 1
- Never silently continue after a failure
- Print all kubectl output during the run (not just on failure) so the user
  can see progress

### After writing the script
1. Test it: run .\scripts\k8s-restore.ps1 from the repo root. The cluster
   may or may not already have resources — the script must be idempotent
   (safe to run whether the namespace exists or not, whether the secret
   exists or not, whether the migration job exists or not).
2. Verify the final summary shows all PASS.
3. Add to .gitignore: nothing (the script has no secrets — it reads from
   .env which is already gitignored).
4. Add a one-liner to README.md under "Run locally" section:
   "After a Kubernetes reset: `.\scripts\k8s-restore.ps1`"
5. Commit: `feat: add k8s-restore.ps1 for one-command cluster recovery`
6. git push origin master
