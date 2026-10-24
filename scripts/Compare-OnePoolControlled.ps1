[CmdletBinding()]
param([Parameter(Mandatory)][string] $ReportPath)

$ErrorActionPreference = 'Stop'
$rows = @(Import-Csv -LiteralPath $ReportPath -Delimiter "`t")
$culture = [Globalization.CultureInfo]::InvariantCulture
function Number($value) { [double]::Parse($value, $culture) }
function Average($values) { ($values | Measure-Object -Average).Average }
function Metric($values, $key) { Average @($values | ForEach-Object { Number $_.$key }) }

$groups = @(
    @{ Workflow = 'Standard'; Runs = @('StandardSimple', 'StandardR', 'CompareStandardS', 'MatchedStandardS') },
    @{ Workflow = 'Soss'; Runs = @('SossSimple', 'SossR', 'CompareSossS', 'MatchedSossS') }
)

Write-Output 'Controlled OnePool comparison'
Write-Output 'Endpoint metrics average equally sized pools. Soss uses Merge evaluations only.'
Write-Output 'Paired comparisons average mutation variants within each replica first; replicas are the independent units.'
Write-Output 'Matched S runs use rate 0.1/7: the expected R flip rate for uniform activators in [-3,3]. Selection can change the R rate later.'
foreach ($group in $groups) {
    Write-Output "`n$($group.Workflow)"
    $perReplica = @{}
    foreach ($run in $group.Runs) {
        $runRows = @($rows | Where-Object { $_.RunName -eq $run -and ($group.Workflow -eq 'Standard' -or $_.SortableTestsSubsetId -eq 'Merge') })
        if ($runRows.Count -eq 0) { throw "Missing $run rows." }
        $generation = ($runRows | ForEach-Object { [int]$_.GenerationNumber } | Measure-Object -Maximum).Maximum
        $endpoint = @($runRows | Where-Object { [int]$_.GenerationNumber -eq $generation })
        $perReplica[$run] = @{}
        foreach ($rep in ($endpoint | Group-Object repl)) {
            $perReplica[$run][$rep.Name] = @{
                CE = Metric $rep.Group 'AveCeLength'
                Stages = Metric $rep.Group 'AveStageLength'
                Score = Metric $rep.Group 'AverageSorterPoolScore'
            }
        }
        $ce = Metric $endpoint 'AveCeLength'
        $stages = Metric $endpoint 'AveStageLength'
        $score = Metric $endpoint 'AverageSorterPoolScore'
        $unsorted = Metric $endpoint 'AverageUnsortedCount'
        $failures = @($endpoint | Where-Object { (Number $_.AverageUnsortedCount) -gt 0 }).Count
        $rates = @($runRows | Select-Object -ExpandProperty ActivatorMutationRate -Unique)
        Write-Output ("{0}: configured activator mutation rate={1}" -f $run, ($rates -join ','))
        Write-Output ("{0}: generation={1}, executions={2}, replicas={3}, CEs={4:F5}, stages={5:F5}, score={6:F5}, averageUnsorted={7:F5}, poolsWithFailures={8}" -f $run, $generation, $endpoint.Count, $perReplica[$run].Count, $ce, $stages, $score, $unsorted, $failures)
        $initial = ($runRows | ForEach-Object { [int]$_.GenerationNumber } | Measure-Object -Minimum).Minimum
        $first = @($runRows | Where-Object { [int]$_.GenerationNumber -eq $initial })
        Write-Output ("  Initial generation={0}, CEs={1:F5}, stages={2:F5}, score={3:F5}, averageUnsorted={4:F5}" -f $initial, (Metric $first 'AveCeLength'), (Metric $first 'AveStageLength'), (Metric $first 'AverageSorterPoolScore'), (Metric $first 'AverageUnsortedCount'))
        foreach ($step in @(10, 20, 50, 100)) {
            $checkpoint = @($runRows | Where-Object { [int]$_.GenerationNumber -eq ($initial + $step - 1) })
            if ($checkpoint.Count -gt 0) {
                Write-Output ("  After {0} reported generations: CEs={1:F5}, stages={2:F5}, score={3:F5}, averageUnsorted={4:F5}" -f $step, (Metric $checkpoint 'AveCeLength'), (Metric $checkpoint 'AveStageLength'), (Metric $checkpoint 'AverageSorterPoolScore'), (Metric $checkpoint 'AverageUnsortedCount'))
            }
        }
        foreach ($replica in ($perReplica[$run].Keys | Sort-Object { [int]$_ })) {
            $metrics = $perReplica[$run][$replica]
            Write-Output ("  Replica {0}: CEs={1:F5}, stages={2:F5}, score={3:F5}" -f $replica, $metrics.CE, $metrics.Stages, $metrics.Score)
        }
    }
    foreach ($pair in @(@(1, 0), @(2, 0), @(2, 1), @(3, 1), @(3, 2))) {
        $left, $right = $group.Runs[$pair[0]], $group.Runs[$pair[1]]
        $replicas = @($perReplica[$right].Keys | Sort-Object { [int]$_ })
        if (($replicas -join ',') -ne (($perReplica[$left].Keys | Sort-Object { [int]$_ }) -join ',')) { throw 'Unpaired replicas.' }
        foreach ($metric in @('CE', 'Stages', 'Score')) {
            $differences = @($replicas | ForEach-Object { $perReplica[$left][$_][$metric] - $perReplica[$right][$_][$metric] })
            $mean = Average $differences
            $wins = @($differences | Where-Object { $_ -lt 0 }).Count
            $range = $differences | Measure-Object -Minimum -Maximum
            Write-Output ("  {0} minus {1}, {2}: mean={3:F5}, range=[{4:F5}, {5:F5}], lower in {6}/{7} replicas" -f $left, $right, $metric, $mean, $range.Minimum, $range.Maximum, $wins, $replicas.Count)
        }
    }
}
