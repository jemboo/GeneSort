[CmdletBinding()]
param(
    [ValidateRange(1, 1024)]
    [int] $MaxParallel = 8,
    [switch] $ListOnly,
    [switch] $ControlledOnly,
    [switch] $Msuf4aSimpleR,
    [switch] $Msuf4aSimpleS,
    [switch] $NoBuild
)

$ErrorActionPreference = 'Stop'
$project = Join-Path $PSScriptRoot 'OnePoolRunner/OnePoolRunner.fsproj'
$runner = Join-Path $PSScriptRoot 'OnePoolRunner/bin/Debug/net10.0/OnePoolRunner.dll'
$auditProject = Join-Path $PSScriptRoot 'OnePoolAudit/OnePoolAudit.fsproj'
$audit = Join-Path $PSScriptRoot 'OnePoolAudit/bin/Debug/net10.0/OnePoolAudit.dll'

try {
    if (@($ControlledOnly, $Msuf4aSimpleR, $Msuf4aSimpleS).Where({ $_ }).Count -gt 1) {
        throw 'Choose only one of -ControlledOnly, -Msuf4aSimpleR, or -Msuf4aSimpleS.'
    }
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
    if ($ControlledOnly) { $runnerArgs = @('--controlled') + $runnerArgs }
    if ($Msuf4aSimpleR) { $runnerArgs = @('--msuf4r') + $runnerArgs }
    if ($Msuf4aSimpleS) { $runnerArgs = @('--msuf4s') + $runnerArgs }
    & dotnet $runner @runnerArgs
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    if ($ListOnly) { exit 0 }

    Write-Host 'Verifying pool sizes, source selections, mutation expansion, and the combined summary...'
    $auditArgs = @('--combine')
    if ($ControlledOnly) { $auditArgs += '--controlled' }
    if ($Msuf4aSimpleR) { $auditArgs += '--msuf4r' }
    if ($Msuf4aSimpleS) { $auditArgs += '--msuf4s' }
    & dotnet $audit @auditArgs
    exit $LASTEXITCODE
}
catch {
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 1
}
