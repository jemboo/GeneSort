[CmdletBinding()]
param([Parameter(Mandatory)][string] $ReportPath)

$ErrorActionPreference = 'Stop'
$rows = @(Import-Csv -LiteralPath $ReportPath -Delimiter "`t")
$culture = [Globalization.CultureInfo]::InvariantCulture
function Number($value) { [double]::Parse($value, $culture) }
function Metric($values, $key) { ($values | ForEach-Object { Number $_.$key } | Measure-Object -Average).Average }

Write-Output 'Msuf4a.SimpleR comparison'
Write-Output 'Sweep endpoints only; averages across four mutation variants of replica 0. These are not four independent seed replicas.'
Write-Output 'Soss uses Merge evaluations. Lower CEs, stages and score are better once sorting succeeds.'
Write-Output 'AMax changes both initial activity (a/(2a+1)) and the initial expected flip rate (mutation rate/(2a+1)).'
Write-Output "Source: $ReportPath"

foreach ($workflow in @('Standard', 'Soss')) {
    $eligible = @($rows | Where-Object { $_.Workflow -eq $workflow -and ($workflow -eq 'Standard' -or $_.SortableTestsSubsetId -eq 'Merge') })
    Write-Output "`n$workflow"
    $test = @($eligible | Where-Object RunName -eq 'Test')
    $testLast = ($test.GenerationNumber | ForEach-Object { [int]$_ } | Measure-Object -Maximum).Maximum
    $testEnd = @($test | Where-Object { [int]$_.GenerationNumber -eq $testLast })
    Write-Output ("Test: generation={0}, executions={1}, CEs={2:F5}, stages={3:F5}, score={4:F5}, unsorted={5:F5}" -f $testLast, $testEnd.Count, (Metric $testEnd 'AveCeLength'), (Metric $testEnd 'AveStageLength'), (Metric $testEnd 'AverageSorterPoolScore'), (Metric $testEnd 'AverageUnsortedCount'))
    $sweep = @($eligible | Where-Object RunName -eq 'ActivatorRates')
    if ($sweep.Count -eq 0) { throw "Missing $workflow sweep." }
    $last = ($sweep.GenerationNumber | ForEach-Object { [int]$_ } | Measure-Object -Maximum).Maximum
    $endpoint = @($sweep | Where-Object { [int]$_.GenerationNumber -eq $last })
    $summaries = @()
    foreach ($group in ($endpoint | Group-Object AMax, ActivatorMutationRate | Sort-Object { [int]$_.Group[0].AMax }, { Number $_.Group[0].ActivatorMutationRate })) {
        $samples = $group.Group
        $aMax = [int]$samples[0].AMax
        $rate = Number $samples[0].ActivatorMutationRate
        $ce = Metric $samples 'AveCeLength'
        $stage = Metric $samples 'AveStageLength'
        $score = Metric $samples 'AverageSorterPoolScore'
        $range = $samples | ForEach-Object { Number $_.AverageSorterPoolScore } | Measure-Object -Minimum -Maximum
        $failedPools = @($samples | Where-Object { (Number $_.AverageUnsortedCount) -gt 0 }).Count
        Write-Output ("AMax={0}, rate={1:F2}: executions={2}, CEs={3:F5}, stages={4:F5}, score={5:F5}, scoreRange=[{6:F5},{7:F5}], poolsWithFailures={8}" -f $aMax, $rate, $samples.Count, $ce, $stage, $score, $range.Minimum, $range.Maximum, $failedPools)
        $summaries += [pscustomobject]@{ AMax=$aMax; Rate=$rate; CE=$ce; Stages=$stage; Score=$score; Failures=$failedPools }
        $trajectory = @($sweep | Where-Object { [int]$_.AMax -eq $aMax -and (Number $_.ActivatorMutationRate) -eq $rate })
        $first = ($trajectory.GenerationNumber | ForEach-Object { [int]$_ } | Measure-Object -Minimum).Minimum
        foreach ($step in @(1, 10, 20, 50, 100)) {
            $checkpoint = @($trajectory | Where-Object { [int]$_.GenerationNumber -eq ($first + $step - 1) })
            if ($checkpoint.Count -gt 0) {
                Write-Output ("  Reported generation {0}: CEs={1:F5}, stages={2:F5}, score={3:F5}, unsorted={4:F5}" -f $step, (Metric $checkpoint 'AveCeLength'), (Metric $checkpoint 'AveStageLength'), (Metric $checkpoint 'AverageSorterPoolScore'), (Metric $checkpoint 'AverageUnsortedCount'))
            }
        }
        foreach ($sample in ($samples | Sort-Object { [int]$_.MutationMod })) {
            Write-Output ("  MutationMod={0}: CEs={1}, stages={2}, score={3}, unsorted={4}" -f $sample.MutationMod, $sample.AveCeLength, $sample.AveStageLength, $sample.AverageSorterPoolScore, $sample.AverageUnsortedCount)
        }
    }
    $best = $summaries | Where-Object Failures -eq 0 | Sort-Object Score | Select-Object -First 1
    Write-Output ("Best observed average score: AMax={0}, rate={1:F2}, score={2:F5}, CEs={3:F5}, stages={4:F5}" -f $best.AMax, $best.Rate, $best.Score, $best.CE, $best.Stages)
    foreach ($aMax in ($summaries.AMax | Sort-Object -Unique)) {
        $bestForBound = $summaries | Where-Object AMax -eq $aMax | Sort-Object Score | Select-Object -First 1
        Write-Output ("Best rate at AMax={0}: {1:F2}, score={2:F5}" -f $aMax, $bestForBound.Rate, $bestForBound.Score)
    }
}
