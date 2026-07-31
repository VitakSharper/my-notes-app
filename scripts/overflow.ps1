<#
.SYNOPSIS
    Starts, stops and inspects the Overflow stack running under Docker Compose.

.DESCRIPTION
    Wraps the docker compose invocations this project needs, which all have to run from
    Overflow.AppHost/infra (compose reads the generated .env sitting next to the file) and under
    the fixed project name "overflow" (that is what the existing volumes are attached to).

.EXAMPLE
    .\scripts\overflow.ps1 start
    .\scripts\overflow.ps1 status
    .\scripts\overflow.ps1 logs question-svc
    .\scripts\overflow.ps1 stop
#>
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('start', 'stop', 'restart', 'status', 'logs', 'build', 'publish', 'down', 'web', 'urls')]
    [string]$Command = 'status',

    # Service name for logs/restart, e.g. question-svc, search-svc, gateway, keycloak, minio.
    [Parameter(Position = 1)]
    [string]$Service,

    # down only: also delete the volumes. That destroys the SQL data, the Keycloak realm
    # (including the nextjs client and kc-admin), the search index and the uploaded images.
    [switch]$DestroyData,

    # logs only: keep streaming instead of printing the last lines and returning.
    [switch]$Follow,

    [int]$Tail = 60
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$infra = Join-Path $repoRoot 'Overflow.AppHost/infra'
$webapp = Join-Path $repoRoot 'webapp'
$project = 'overflow'

# Images the SDK builds (there is no Dockerfile): kept in sync with the compose file.
$serviceImages = @{
    'question-svc' = Join-Path $repoRoot 'QuestionService/QuestionService.csproj'
    'search-svc'   = Join-Path $repoRoot 'SearchService/SearchService.csproj'
}

# Only what compose actually publishes on the host: SQL, RabbitMQ and Typesense are `expose`d to
# the aspire network only, so they are reachable from the services but not from here.
$endpoints = [ordered]@{
    'Gateway (API)'    = 'http://localhost:8001/questions'
    'Gateway (by host)'= 'http://api.overflow.local/questions'
    'Keycloak'         = 'http://id.overflow.local/realms/overflow'
    'MinIO (S3)'       = 'http://localhost:9000/minio/health/live'
    'MinIO console'    = 'http://localhost:9001'
    'Aspire dashboard' = 'http://localhost:8080'
    'Client app'       = 'http://localhost:3000/questions'
}

function Write-Step($message) { Write-Host "==> $message" -ForegroundColor Cyan }
function Write-Warn($message) { Write-Host "  ! $message" -ForegroundColor Yellow }

function Invoke-Compose {
    param([string[]]$Arguments)

    Push-Location $infra
    try {
        & docker compose -p $project @Arguments
        if ($LASTEXITCODE -ne 0) { throw "docker compose $($Arguments -join ' ') exited with $LASTEXITCODE" }
    }
    finally { Pop-Location }
}

function Assert-Prerequisites {
    if (-not (Test-Path (Join-Path $infra 'docker-compose.yaml'))) {
        throw "No compose file in $infra. Run '.\scripts\overflow.ps1 publish' first."
    }

    $envFile = Join-Path $infra '.env'
    if (-not (Test-Path $envFile)) {
        throw "No .env in $infra. It is gitignored (it holds the container passwords): regenerate it with 'publish', then fill MINIO_USER and MINIO_PASSWORD from 'dotnet user-secrets list --project Overflow.AppHost'."
    }

    # aspire publish writes empty placeholders for parameters declared with AddParameter.
    $empty = Select-String -Path $envFile -Pattern '^(MINIO_USER|MINIO_PASSWORD)=\s*$' -ErrorAction SilentlyContinue
    if ($empty) {
        Write-Warn "$($empty.Count) MinIO value(s) are empty in infra/.env - image upload will fail with InvalidAccessKeyId. Fill them from the AppHost user secrets."
    }

    foreach ($image in $serviceImages.Keys) {
        & docker image inspect "$($image):latest" *> $null
        if ($LASTEXITCODE -ne 0) {
            Write-Warn "Image '$image:latest' is missing - run '.\scripts\overflow.ps1 build'."
        }
    }

    $hosts = Get-Content "$env:SystemRoot\System32\drivers\etc\hosts" -ErrorAction SilentlyContinue
    foreach ($name in @('api.overflow.local', 'id.overflow.local')) {
        if (-not ($hosts | Select-String -SimpleMatch $name -Quiet)) {
            Write-Warn "'$name' is not in your hosts file - nginx-proxy routes by Host header, so Keycloak and the gateway will not resolve."
        }
    }
}

function Get-ContainerStatus {
    Push-Location $infra
    try { $raw = & docker compose -p $project ps --format json 2>$null }
    finally { Pop-Location }

    if (-not $raw) { return @() }

    # docker compose emits one JSON object per line.
    return $raw | Where-Object { $_ } | ForEach-Object { $_ | ConvertFrom-Json }
}

function Wait-ForReady {
    param([int]$TimeoutSeconds = 150)

    # SQL Server is the slow one and question-svc waits for it, so the gateway only stops
    # answering 502 once the whole chain is up: checking it covers the services behind it.
    $checks = [ordered]@{
        'keycloak' = 'http://id.overflow.local/realms/overflow'
        'minio'    = 'http://localhost:9000/minio/health/live'
        'gateway'  = 'http://localhost:8001/questions'
    }

    $pending = [System.Collections.Generic.List[string]]::new()
    $checks.Keys | ForEach-Object { $pending.Add($_) }

    Write-Step "Waiting for the services (up to ${TimeoutSeconds}s)"
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)

    while ($pending.Count -gt 0 -and (Get-Date) -lt $deadline) {
        foreach ($name in @($pending)) {
            $ok = try {
                (Invoke-WebRequest -Uri $checks[$name] -TimeoutSec 3 -SkipHttpErrorCheck -ErrorAction Stop).StatusCode -lt 500
            }
            catch { $false }

            if ($ok) {
                Write-Host "  $name ready" -ForegroundColor Green
                $pending.Remove($name) | Out-Null
            }
        }

        if ($pending.Count -gt 0) { Start-Sleep -Seconds 3 }
    }

    if ($pending.Count -gt 0) {
        Write-Warn "Still not answering: $($pending -join ', '). Check '.\scripts\overflow.ps1 logs <service>'."
    }
}

function Show-Status {
    $containers = Get-ContainerStatus

    if (-not $containers) {
        Write-Host 'No container for project "overflow" - nothing has been created yet.' -ForegroundColor DarkGray
    }
    else {
        $containers |
            Sort-Object Service |
            Format-Table -AutoSize @{ n = 'Service'; e = { $_.Service } },
                                   @{ n = 'State'; e = { $_.State } },
                                   @{ n = 'Status'; e = { $_.Status } },
                                   # 0 means "not published on the host", and each protocol shows
                                   # up separately, hence the filter and the dedupe.
                                   @{ n = 'Host ports'; e = {
                                        ($_.Publishers.PublishedPort |
                                            Where-Object { $_ -and $_ -ne 0 } |
                                            Select-Object -Unique |
                                            Sort-Object) -join ',' } }
    }

    Write-Step 'Endpoints'
    foreach ($entry in $endpoints.GetEnumerator()) {
        # GET, not HEAD: the API only allows GET on these routes and would answer 405.
        $code = try {
            (Invoke-WebRequest -Uri $entry.Value -TimeoutSec 4 -SkipHttpErrorCheck -ErrorAction Stop).StatusCode
        }
        catch { 'unreachable' }

        $colour = if ($code -is [int] -and $code -lt 500) { 'Green' } else { 'DarkGray' }
        Write-Host ('  {0,-18} {1,-45} {2}' -f $entry.Key, $entry.Value, $code) -ForegroundColor $colour
    }

    if (-not ($containers | Where-Object { $_.Service -eq 'webapp' })) {
        Write-Host ''
        Write-Warn "The client app is not part of the compose file yet (it needs PublishAsDockerFile, which section 11 covers). Use '.\scripts\overflow.ps1 web' to run it with npm."
    }
}

switch ($Command) {
    'build' {
        foreach ($entry in $serviceImages.GetEnumerator()) {
            Write-Step "Building image $($entry.Key)"
            & dotnet publish $entry.Value -c Release -t:PublishContainer `
                -p:ContainerRepository=$($entry.Key) -p:ContainerImageTag=latest
            if ($LASTEXITCODE -ne 0) { throw "Build of $($entry.Key) failed" }
        }
        Write-Host 'Images built. Run "start" to pick them up.' -ForegroundColor Green
    }

    'publish' {
        # Regenerates infra/docker-compose.yaml from AppHost.cs. Needed after changing the AppHost
        # (a new container, a new YARP route, a new environment variable).
        Write-Step 'Rendering the compose file from the AppHost'
        $aspire = Join-Path $env:USERPROFILE '.dotnet/tools/aspire.cmd'
        if (-not (Test-Path $aspire)) { throw "Aspire CLI not found at $aspire" }

        Push-Location $repoRoot
        try {
            & $aspire publish -o Overflow.AppHost/infra --non-interactive
            if ($LASTEXITCODE -ne 0) { throw 'aspire publish failed' }
        }
        finally { Pop-Location }

        Write-Warn 'Check infra/.env: parameters declared with AddParameter are written as empty placeholders.'
    }

    'start' {
        Assert-Prerequisites
        Write-Step 'Starting the stack'
        Invoke-Compose @('up', '-d')
        Write-Host ''
        Wait-ForReady
        Write-Host ''
        Show-Status
    }

    'stop' {
        Write-Step 'Stopping the containers (kept, so the next start is fast)'
        Invoke-Compose @('stop')
    }

    'restart' {
        if ($Service) {
            Write-Step "Restarting $Service"
            Invoke-Compose @('restart', $Service)
        }
        else {
            Write-Step 'Restarting every container'
            Invoke-Compose @('restart')
        }
    }

    'logs' {
        # Not following by default: a script that never returns is a poor default.
        $arguments = @('logs', '--tail', "$Tail")
        if ($Follow) { $arguments += '-f' }
        if ($Service) { $arguments += $Service }
        Invoke-Compose $arguments
    }

    'down' {
        if ($DestroyData) {
            Write-Host 'This deletes the volumes: SQL data, the Keycloak realm (nextjs client and kc-admin included), the search index and the uploaded images.' -ForegroundColor Red
            $answer = Read-Host 'Type DESTROY to confirm'
            if ($answer -ne 'DESTROY') { Write-Host 'Cancelled.'; break }

            Write-Step 'Removing the containers and their volumes'
            Invoke-Compose @('down', '-v')
        }
        else {
            Write-Step 'Removing the containers (volumes kept)'
            Invoke-Compose @('down')
        }
    }

    'web' {
        $containers = Get-ContainerStatus
        if ($containers | Where-Object { $_.Service -eq 'webapp' }) {
            Write-Step 'The client app is in the compose file: starting its container'
            Invoke-Compose @('up', '-d', 'webapp')
            break
        }

        if (-not (Test-Path (Join-Path $webapp 'node_modules'))) {
            Write-Step 'Installing the client dependencies'
            Push-Location $webapp
            try { & npm install } finally { Pop-Location }
        }

        Write-Step 'Starting next dev in its own window (http://localhost:3000)'
        Start-Process -FilePath 'cmd.exe' -ArgumentList '/k', 'npm run dev' -WorkingDirectory $webapp
    }

    'urls' {
        foreach ($entry in $endpoints.GetEnumerator()) {
            Write-Host ('  {0,-18} {1}' -f $entry.Key, $entry.Value)
        }
    }

    default { Show-Status }
}
