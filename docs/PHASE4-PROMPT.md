# Phase 4 Prompt — paste into Claude Code at E:\Projects\inherited\FinanceApp

Phase 3 complete. Read CLAUDE.md first. Phase 4 = Kubernetes manifests for the
local Docker Desktop cluster (kubectl context docker-desktop, local registry
localhost:5555). Plain manifests in k8s/ — no Helm (Helm is showcased in a
different portfolio project; keep this one kubectl-native).

## Step 0 — Pre-flight
1. Verify cluster: `kubectl config current-context` is docker-desktop and
   nodes are Ready. Verify the local registry answers on :5555. If either
   fails, STOP and report — don't improvise infrastructure.
2. The compose stack may be running on 8888 — leave it; k8s exposure uses
   8889 to coexist (decision below).

## Step 1 — Image to local registry
1. Tag and push: localhost:5555/financeapp:p4 (and :latest)
2. Pull-test the tag before writing manifests.

## Step 2 — Manifests (k8s/, numbered files for apply order)
1. namespace.yaml: financeapp
2. postgres: StatefulSet (postgres:16-alpine) + headless Service + PVC;
   credentials from a Secret (see Step 3); pg_isready in readiness probe
3. Migration Job (THIS fulfills the Phase 2 promise that "production uses a
   migration job"): decide between (a) EF migration bundle (efbundle) built
   in a Dockerfile stage, or (b) the app image with a run-migrations-and-exit
   mode. Verify both approaches against current EF Core 8 docs before
   choosing; state the tradeoff in a manifest comment. App pods set
   RUN_MIGRATIONS=false in k8s — the Job is the only migrator.
4. app Deployment: 1 replica (DataProtection keys on a PVC make >1 replica
   incorrect for now — note this limitation in a comment; fixing it = shared
   key storage, out of scope), resources requests/limits, env from ConfigMap
   + Secret, image localhost:5555/financeapp:p4
5. Probes: SPLIT health endpoints first in code — /health/live (no
   dependencies, liveness) and /health/ready (includes the Npgsql check,
   readiness). A liveness probe that depends on the DB causes restart
   cascades when the DB blips — add a code comment saying exactly that.
   Small code change + test, include in this phase's commit.
6. app Service: type LoadBalancer, port 8889 → 8080 (Docker Desktop maps
   LoadBalancer to localhost; 8888 stays with the compose stack)

## Step 3 — Secrets & config (NEVER committed)
1. k8s/secret.example.yaml committed with placeholder names only; real secret
   created imperatively and documented in a comment:
   kubectl -n financeapp create secret generic financeapp-secrets
     --from-literal=ConnectionStrings__DefaultConnection=...
     --from-literal=Anthropic__ApiKey=... (etc.)
2. ConfigMap for non-secret env: ASPNETCORE_ENVIRONMENT, RUN_MIGRATIONS=false,
   SEED_DEMO_DATA=true (demo data is the point of this cluster), Serilog level
3. Verify .gitignore: any file matching k8s/*secret*.yaml except the example

## Step 4 — Deploy & verify (show all outputs)
1. kubectl apply in numbered order; migration Job runs to Completion BEFORE
   the app Deployment is applied — show `kubectl get jobs,pods -n financeapp`
2. `kubectl rollout status deployment/financeapp -n financeapp`
3. curl http://localhost:8889/health/ready → Healthy; /health/live → Healthy
4. Real login against the seeded demo user via :8889, dashboard renders
5. Kill the postgres pod, show that: liveness does NOT restart the app pod,
   readiness marks it NotReady, and recovery is automatic when postgres
   returns — this is the probe-split payoff, capture the output for the
   Phase 6 demo material
6. Compose stack on 8888 still works side-by-side (docker compose ps)

## Step 5 — Wrap-up
1. Update CLAUDE.md Build Progress: Phase 4 complete; record the migration
   Job approach chosen (bundle vs app-image flag) and why; note the
   single-replica DataProtection limitation as known debt
2. docs/PHASE4-NOTES.md: one page — apply order, secret creation command
   shape (no values), probe philosophy, how to tear down
   (kubectl delete ns financeapp)
3. Conventional commit; push after EVERY commit in this phase, not just the
   last one (CLAUDE.md rule)
