[CmdletBinding()]
param(
    [ValidateRange(1, 1024)]
    [int] $MaxParallel = 8,
    [switch] $ListOnly,
    [switch] $NoBuild
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'OnePoolRunner/OnePoolRunner.fsproj'
$runner = Join-Path $PSScriptRoot 'OnePoolRunner/bin/Debug/net10.0/OnePoolRunner.dll'
$auditProject = Join-Path $PSScriptRoot 'OnePoolAudit/OnePoolAudit.fsproj'
$audit = Join-Path $PSScriptRoot 'OnePoolAudit/bin/Debug/net10.0/OnePoolAudit.dll'

try {
    if (-not $NoBuild) {
        & dotnet build $project --configuration Debug --verbosity quiet -m:1
        if ($LASTEXITCODE -ne 0) { throw "OnePool runner build failed (exit $LASTEXITCODE)." }
        if (-not $ListOnly) {
            & dotnet build $auditProject --configuration Debug --verbosity quiet -m:1
            if ($LASTEXITCODE -ne 0) { throw "OnePool verification build failed (exit $LASTEXITCODE)." }
        }
    }

    $runnerArgs = @([string] $MaxParallel)
    if ($ListOnly) { $runnerArgs = @('--list') + $runnerArgs }
    & dotnet $runner @runnerArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    if ($ListOnly) { exit 0 }

    Write-Host 'Verifying Append pool sizes and Condense source selections...'
    & dotnet $audit --combine
    exit $LASTEXITCODE
}
catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 1
}
