# Phase 4 — Kubernetes Manifests

**Date**: 2026-06-15
**Branch**: master
**Commit**: see `git log --oneline -1`

## What was built

Six manifests under `k8s/` deploying the full stack to a local Docker Desktop
Kubernetes cluster (context: `docker-desktop`, node: `desktop-control-plane`).

| File | Purpose |
|---|---|
| `00-namespace.yaml` | `financeapp` namespace |
| `01-configmap.yaml` | Non-secret env vars (`ASPNETCORE_ENVIRONMENT`, `RUN_MIGRATIONS`, `SEED_DEMO_DATA`, log levels) |
| `02-postgres.yaml` | Headless Service + PVC (2Gi) + StatefulSet `postgres:16-alpine` |
| `03-secret.example.yaml` | Template for `kubectl create secret generic` — never committed with real values |
| `04-migration-job.yaml` | `batch/v1 Job` with init container (`pg_isready` loop) + migrate container (`MIGRATIONS_ONLY=true`) |
| `05-app.yaml` | PVC (64Mi DataProtection keys) + Deployment (1 replica, RWO PVC limit) + LoadBalancer Service `8889:8080` |

## Apply order

```
kubectl apply -f k8s/00-namespace.yaml
kubectl apply -f k8s/01-configmap.yaml
kubectl apply -f k8s/02-postgres.yaml
# create secret imperatively (see 03-secret.example.yaml)
kubectl apply -f k8s/04-migration-job.yaml
kubectl wait --for=condition=complete job/financeapp-migrate -n financeapp --timeout=120s
kubectl apply -f k8s/05-app.yaml
```

## Key decisions

**a. Probe split — liveness vs. readiness**
- `/health/live` (no checks, always Healthy if process responds) drives liveness.
  A DB-dependent liveness probe causes restart cascades when the DB blips:
  Kubernetes kills and restarts the pod, which also can't reach the DB → tight
  restart loop. Liveness = "is the process stuck?", not "are all deps reachable?"
- `/health/ready` (Npgsql tag `"ready"`) drives readiness. When the DB is
  unreachable the pod is removed from Service endpoints but NOT restarted;
  it recovers automatically when the DB returns.

**b. Migration Job, not in-process**
- The same image (`localhost:5555/financeapp:p4`) runs with `MIGRATIONS_ONLY=true`
  so it applies EF migrations and exits 0. App pods set `RUN_MIGRATIONS=false`.
  Tradeoff vs. efbundle: efbundle is purpose-built but needs an extra Dockerfile
  stage. The env-flag approach reuses the published artifact — right call for a
  portfolio demo cluster where migrations run rarely.

**c. DataProtection PVC**
- A 64Mi RWO PVC at `/home/app/.aspnet/DataProtection-Keys` persists keys across
  pod recreations so auth cookies and antiforgery tokens survive deployments.
- Replica cap: 1. Multiple replicas sharing no key material would issue different
  tokens per pod → auth failures. Fix = shared key storage (Azure Blob / Redis /
  k8s Secret-backed provider) — out of scope for this portfolio phase.

**d. Secret management**
- All credentials (`POSTGRES_*`, `ConnectionStrings__DefaultConnection`,
  `Anthropic__ApiKey`, `SendGrid__ApiKey`, `SendGrid__SenderEmail`,
  `SeedDemo__Password`) live in a `financeapp-secrets` generic Secret created
  imperatively. `03-secret.example.yaml` documents the `kubectl create` command;
  the file itself holds only `<PLACEHOLDER>` values and is safe to commit.

**e. Port**
- Service: `LoadBalancer 8889:8080`. Docker Desktop maps LoadBalancer services
  to localhost automatically. 8889 coexists with the compose stack on 8888.
- Container port 8080 is the `aspnet:8.0` image default via `ASPNETCORE_HTTP_PORTS`.

**f. postgres headless Service**
- `clusterIP: None` gives the StatefulSet a stable DNS name (`postgres.financeapp
  .svc.cluster.local`) that the connection string uses — no ClusterIP needed
  since nothing load-balances to postgres.

## Verified

- `kubectl get nodes` → `desktop-control-plane Ready`
- Migration Job → `Complete 1/1` (0 restarts)
- App pod → `Running 1/1` (0 restarts)
- `GET http://localhost:8889/health/live` → `200 Healthy`
- `GET http://localhost:8889/health/ready` → `200 Healthy`

## Postgres resilience test

**Setup**: both probes confirmed healthy, then `kubectl delete pod postgres-0`.

**Result** (verified via `kubectl describe pod` events):
- App pod **Restart Count: 0** throughout — liveness probe (`/health/live`,
  no DB dependency) never fired a kill signal. Probe design achieved its goal.
- Readiness probe fired twice: `503` (DB unreachable) then `context deadline
  exceeded` (postgres mid-restart). Events:
  ```
  Warning  Unhealthy  Readiness probe failed: HTTP probe failed with statuscode: 503
  Warning  Unhealthy  Readiness probe failed: context deadline exceeded
  ```
- StatefulSet recreated `postgres-0` in <10s. Because recovery was faster than
  `failureThreshold(3) × period(10s) = 30s`, the pod never fully left the Ready
  state in the `kubectl get pods` column — but the probe failure events confirm
  the readiness gate fired exactly as designed.
- Both `/health/live` and `/health/ready` returned `200 Healthy` immediately
  after postgres recovered.

**Conclusion**: the liveness/readiness split works correctly. A real outage
lasting >30s would remove the pod from Service endpoints (no new traffic) while
leaving it alive to recover — no restart cascade.

## Audit items resolved in Phase 4

| Item | Description |
|---|---|
| Phase 2 promise | Production migration job (not in-process) — fulfilled |
| Phase 3 deferred | TLS termination — deferred to Phase 5/6 (out of scope for local portfolio cluster) |
