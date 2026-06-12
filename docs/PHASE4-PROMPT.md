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
