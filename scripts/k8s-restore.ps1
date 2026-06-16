#Requires -Version 7
<#
.SYNOPSIS
    Restores the FinTrak k8s stack after a Docker Desktop Kubernetes reset.
.DESCRIPTION
    Idempotent one-command restore. Reads secrets from .env, recreates the
    financeapp-secrets Secret, and deploys all manifests in the correct order.
    Run from the repo root: .\scripts\k8s-restore.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot  = Split-Path $PSScriptRoot -Parent
$EnvFile   = Join-Path $RepoRoot '.env'
$K8sDir    = Join-Path $RepoRoot 'k8s'
$Docker    = 'C:\Program Files\Docker\Docker\resources\bin\docker.exe'
$StartTime = [datetime]::Now

function Log {
    param([string]$Msg, [string]$Color = 'Cyan')
    $ts = [datetime]::Now.ToString('HH:mm:ss')
    Write-Host "[$ts] $Msg" -ForegroundColor $Color
}

function Err {
    param([string]$Msg)
    Write-Host "`n[ERROR] $Msg" -ForegroundColor Red
    exit 1
}

function Pass { Write-Host "  PASS" -ForegroundColor Green -NoNewline; Write-Host " $_" }
function Fail { Write-Host "  FAIL" -ForegroundColor Red   -NoNewline; Write-Host " $_"; $script:AnyFail = $true }

# ─────────────────────────────────────────────────────────────────────────────
# 0. PREREQUISITES
# ─────────────────────────────────────────────────────────────────────────────
Log "=== FinTrak k8s Restore ===" 'Yellow'
Log "Step 0: Checking prerequisites..."

# kubectl context
$ctx = kubectl config current-context 2>&1
if ($ctx -ne 'docker-desktop') {
    Err "kubectl context is '$ctx', expected 'docker-desktop'. Run: kubectl config use-context docker-desktop"
}
Log "  Context: docker-desktop" 'Green'

# Node Ready (poll up to 30s)
Log "  Waiting for node desktop-control-plane to be Ready..."
$deadline = [datetime]::Now.AddSeconds(30)
$nodeReady = $false
while ([datetime]::Now -lt $deadline) {
    $status = kubectl get node desktop-control-plane --no-headers 2>$null | ForEach-Object { ($_ -split '\s+')[1] }
    if ($status -eq 'Ready') { $nodeReady = $true; break }
    Start-Sleep -Seconds 3
}
if (-not $nodeReady) { Err "Node desktop-control-plane is not Ready after 30s. Is Docker Desktop running?" }
Log "  Node: Ready" 'Green'

# Registry on :5555
Log "  Checking registry on localhost:5555..."
$registryOk = $false
try { Invoke-RestMethod http://localhost:5555/v2/ | Out-Null; $registryOk = $true } catch {}
if (-not $registryOk) {
    Log "  Registry not responding — starting container 'registry'..." 'Yellow'
    & $Docker start registry | Out-Null
    Start-Sleep -Seconds 5
    try { Invoke-RestMethod http://localhost:5555/v2/ | Out-Null; $registryOk = $true } catch {}
}
if (-not $registryOk) { Err "Registry still not responding on :5555 after start attempt." }
Log "  Registry: OK" 'Green'

# ─────────────────────────────────────────────────────────────────────────────
# 1. PARSE .env
# ─────────────────────────────────────────────────────────────────────────────
Log "Step 1: Reading secrets from .env..."

if (-not (Test-Path $EnvFile)) { Err ".env not found at $EnvFile. Copy .env.example and fill in your Anthropic API key." }

$envVars = @{}
$lines   = Get-Content $EnvFile
$i       = 0
while ($i -lt $lines.Count) {
    $line = $lines[$i].Trim()
    # Skip blank lines and comments
    if ($line -eq '' -or $line.StartsWith('#')) { $i++; continue }

    if ($line -match '^([A-Z0-9_]+)=(.*)$') {
        $key = $Matches[1]
        $val = $Matches[2].Trim()
        # Handle broken format: KEY=\n<value on next line>
        if ($val -eq '' -and ($i + 1) -lt $lines.Count) {
            $nextLine = $lines[$i + 1].Trim()
            if ($nextLine -ne '' -and -not $nextLine.StartsWith('#') -and -not ($nextLine -match '^[A-Z0-9_]+=')) {
                $val = $nextLine
                $i++
            }
        }
        $envVars[$key] = $val
    }
    $i++
}

# Required
$anthropicKey = $envVars['ANTHROPIC_API_KEY']
if ([string]::IsNullOrWhiteSpace($anthropicKey)) {
    Err "ANTHROPIC_API_KEY is missing or empty in .env. Add your key from console.anthropic.com."
}
Log "  ANTHROPIC_API_KEY: found" 'Green'

# Optional with defaults/warnings
$seedPassword = $envVars['SEED_DEMO_PASSWORD']
if ([string]::IsNullOrWhiteSpace($seedPassword)) {
    Log "  SEED_DEMO_PASSWORD not set — using default: Demo!Fintrak2026" 'Yellow'
    $seedPassword = 'Demo!Fintrak2026'
} else {
    Log "  SEED_DEMO_PASSWORD: found" 'Green'
}

$sendGridKey   = if ($envVars.ContainsKey('SENDGRID_API_KEY'))    { $envVars['SENDGRID_API_KEY'] }    else { '' }
$sendGridEmail = if ($envVars.ContainsKey('SENDGRID_SENDER_EMAIL')) { $envVars['SENDGRID_SENDER_EMAIL'] } else { '' }
Log "  SENDGRID_API_KEY: $(if ($sendGridKey) { 'found' } else { 'not set (email send disabled)' })" 'Gray'
Log "  SENDGRID_SENDER_EMAIL: $(if ($sendGridEmail) { $sendGridEmail } else { 'not set' })" 'Gray'

# ─────────────────────────────────────────────────────────────────────────────
# 2. DEPLOY
# ─────────────────────────────────────────────────────────────────────────────

# ── Step 1: Namespace + ConfigMap ──────────────────────────────────────────
Log "Step 2a: Applying namespace and configmap..."
kubectl apply -f "$K8sDir\00-namespace.yaml"
if ($LASTEXITCODE -ne 0) { Err "Failed to apply 00-namespace.yaml" }
kubectl apply -f "$K8sDir\01-configmap.yaml"
if ($LASTEXITCODE -ne 0) { Err "Failed to apply 01-configmap.yaml" }

# ── Step 2: Secret (dry-run + apply = create-or-update, no delete needed) ──
Log "Step 2b: Creating/updating financeapp-secrets..."
$connStr = 'Host=postgres;Port=5432;Database=financeapp;Username=financeapp;Password=financeapp_dev_pw'

kubectl -n financeapp create secret generic financeapp-secrets `
    "--from-literal=ConnectionStrings__DefaultConnection=$connStr" `
    "--from-literal=Anthropic__ApiKey=$anthropicKey" `
    "--from-literal=SeedDemo__Password=$seedPassword" `
    "--from-literal=SendGrid__ApiKey=$sendGridKey" `
    "--from-literal=SendGrid__SenderEmail=$sendGridEmail" `
    "--from-literal=POSTGRES_USER=financeapp" `
    "--from-literal=POSTGRES_PASSWORD=financeapp_dev_pw" `
    "--from-literal=POSTGRES_DB=financeapp" `
    --save-config --dry-run=client -o yaml | kubectl apply -f -
if ($LASTEXITCODE -ne 0) { Err "Failed to create/update financeapp-secrets" }

# ── Step 3: PostgreSQL ─────────────────────────────────────────────────────
Log "Step 2c: Applying postgres StatefulSet..."
kubectl apply -f "$K8sDir\02-postgres.yaml"
if ($LASTEXITCODE -ne 0) { Err "Failed to apply 02-postgres.yaml" }

Log "  Waiting for postgres-0 to be Ready (up to 90s)..." 'Gray'
$deadline = [datetime]::Now.AddSeconds(90)
$pgReady  = $false
Write-Host -NoNewline "  "
while ([datetime]::Now -lt $deadline) {
    $status = kubectl get pod postgres-0 -n financeapp --no-headers 2>$null |
              ForEach-Object { ($_.Trim() -split '\s+')[1] }
    if ($status -eq '1/1') { $pgReady = $true; break }
    Write-Host -NoNewline '.'
    Start-Sleep -Seconds 5
}
Write-Host ""
if (-not $pgReady) {
    Log "  postgres-0 pod logs:" 'Red'
    kubectl logs postgres-0 -n financeapp 2>&1 | Write-Host
    Err "postgres-0 not Ready after 90s"
}
Log "  postgres-0: Ready" 'Green'

# ── Step 4: Migration job ──────────────────────────────────────────────────
Log "Step 2d: Running migration job..."
kubectl delete job financeapp-migrate -n financeapp --ignore-not-found
kubectl apply -f "$K8sDir\04-migration-job.yaml"
if ($LASTEXITCODE -ne 0) { Err "Failed to apply 04-migration-job.yaml" }

Log "  Waiting for migration job to complete (up to 180s)..." 'Gray'
$deadline  = [datetime]::Now.AddSeconds(180)
$jobDone   = $false
Write-Host -NoNewline "  "
while ([datetime]::Now -lt $deadline) {
    # Primary signal: COMPLETIONS column shows 1/1
    $jobStatus = kubectl get job financeapp-migrate -n financeapp --no-headers 2>$null |
                 ForEach-Object { ($_.Trim() -split '\s+')[1] }
    if ($jobStatus -eq '1/1') { $jobDone = $true; break }
    # Fallback: job pod itself is Completed (status can lag the pod by a few seconds)
    $podPhase = kubectl get pod -n financeapp -l job-name=financeapp-migrate --no-headers 2>$null |
                ForEach-Object { ($_.Trim() -split '\s+')[2] }
    if ($podPhase -eq 'Completed') { $jobDone = $true; break }
    Write-Host -NoNewline '.'
    Start-Sleep -Seconds 5
}
Write-Host ""
if (-not $jobDone) {
    Log "  Migration job logs:" 'Red'
    kubectl logs job/financeapp-migrate -n financeapp 2>&1 | Write-Host
    Err "Migration job did not complete within 180s"
}
Log "  Migrations: Complete" 'Green'

# ── Step 5: App deployment ─────────────────────────────────────────────────
Log "Step 2e: Deploying app..."
kubectl apply -f "$K8sDir\05-app.yaml"
if ($LASTEXITCODE -ne 0) { Err "Failed to apply 05-app.yaml" }

Log "  Waiting for rollout (up to 120s)..." 'Gray'
kubectl rollout status deployment/financeapp -n financeapp --timeout=120s
if ($LASTEXITCODE -ne 0) {
    Log "  App pod logs:" 'Red'
    kubectl logs -l app=financeapp -n financeapp --tail=50 2>&1 | Write-Host
    Err "financeapp deployment rollout failed"
}
Log "  Deployment: rolled out" 'Green'

# Give the health endpoints a moment to initialise
Start-Sleep -Seconds 5

# ─────────────────────────────────────────────────────────────────────────────
# 3. VERIFY
# ─────────────────────────────────────────────────────────────────────────────
Log "Step 3: Verifying stack..."
$script:AnyFail = $false

# /health/live
try {
    $r = Invoke-RestMethod 'http://localhost:8889/health/live' -TimeoutSec 10
    if ($r -match 'Healthy') { $_ = '/health/live → Healthy'; Pass } else { $_ = "/health/live → unexpected: $r"; Fail }
} catch { $_ = "/health/live → request failed: $_"; Fail }

# /health/ready
try {
    $r = Invoke-RestMethod 'http://localhost:8889/health/ready' -TimeoutSec 10
    if ($r -match 'Healthy') { $_ = '/health/ready → Healthy'; Pass } else { $_ = "/health/ready → unexpected: $r"; Fail }
} catch { $_ = "/health/ready → request failed: $_"; Fail }

# / content check
try {
    $html = Invoke-RestMethod 'http://localhost:8889/' -TimeoutSec 10
    if ($html -match 'Track Spending') { $_ = '/ → contains "Track Spending"'; Pass } else { $_ = '/ → "Track Spending" NOT found in response'; Fail }
} catch { $_ = "/ → request failed: $_"; Fail }

# /Account/Login HTTP 200
try {
    $resp = Invoke-WebRequest 'http://localhost:8889/Account/Login' -TimeoutSec 10 -UseBasicParsing
    if ($resp.StatusCode -eq 200) { $_ = "/Account/Login → HTTP $($resp.StatusCode)"; Pass } else { $_ = "/Account/Login → HTTP $($resp.StatusCode)"; Fail }
} catch { $_ = "/Account/Login → request failed: $_"; Fail }

# ─────────────────────────────────────────────────────────────────────────────
# 4. SUMMARY
# ─────────────────────────────────────────────────────────────────────────────
$elapsed = [datetime]::Now - $StartTime
$mins    = [int]$elapsed.TotalMinutes
$secs    = $elapsed.Seconds

$summaryColor = if ($script:AnyFail) { 'Red' } else { 'Green' }
$summaryIcon  = if ($script:AnyFail) { '⚠️ ' } else { '✅' }
$summaryTitle = if ($script:AnyFail) { 'FinTrak restored with verification failures' } else { 'FinTrak k8s stack restored successfully' }

Write-Host ""
Write-Host ("─" * 55) -ForegroundColor $summaryColor
Write-Host "  $summaryIcon $summaryTitle" -ForegroundColor $summaryColor
Write-Host ("─" * 55) -ForegroundColor $summaryColor
Write-Host "  App:       http://localhost:8889"
Write-Host "  Demo user: demo@fintrak.example / $seedPassword"
Write-Host "  Health:    http://localhost:8889/health/ready"
Write-Host "  Time:      ${mins}m ${secs}s"
Write-Host ("─" * 55) -ForegroundColor $summaryColor

if ($script:AnyFail) { exit 1 }
