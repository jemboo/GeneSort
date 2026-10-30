[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $ReportPath,
    [Parameter(Mandatory)][string] $SimpleRReportPath,
    [ValidateSet('ActivatorRates', 'ActivatorRates32', 'ActivatorRates64')]
    [string] $SimpleSStandardSweep = 'ActivatorRates32'
)

$ErrorActionPreference = 'Stop'
$culture = [Globalization.CultureInfo]::InvariantCulture
function Number($value) { [double]::Parse($value, $culture) }
function Metric($values, $key) { ($values | ForEach-Object { Number $_.$key } | Measure-Object -Average).Average }
$reports = @{
    SimpleS = @(Import-Csv -LiteralPath $ReportPath -Delimiter "`t")
    SimpleR = @(Import-Csv -LiteralPath $SimpleRReportPath -Delimiter "`t")
}
$summaries = @()

Write-Output 'Msuf4a.SimpleS activation and mutation comparison, with SimpleR reference'
Write-Output 'Averages across mutation variants of replica 0, not independent seed replicas. Soss uses Merge evaluations.'
Write-Output 'Lower CEs, stages and score are better once sorting succeeds. These reports do not measure execution speed.'
Write-Output 'S activation probability controls initial activity; its mutation rate is the probability of flipping each boolean.'
Write-Output 'R initial activity is a/(2a+1); its initial expected flip probability is mutation rate/(2a+1).'
Write-Output 'These sweeps share prefix prerequisites and evolution settings, but do not match initial masks or effective flip rates.'
Write-Output "SimpleS source: $ReportPath"
Write-Output "SimpleR source: $SimpleRReportPath"

foreach ($family in @('SimpleS', 'SimpleR')) {
    $settingKey = if ($family -eq 'SimpleS') { 'ActivationProbability' } else { 'AMax' }
    foreach ($workflow in @('Standard', 'Soss')) {
        $eligible = @($reports[$family] | Where-Object { $_.Workflow -eq $workflow -and ($workflow -eq 'Standard' -or $_.SortableTestsSubsetId -eq 'Merge') })
        Write-Output "`n$family $workflow"
        $test = @($eligible | Where-Object RunName -eq 'Test')
        if ($test.Count -eq 0) { throw "Missing $family $workflow Test report." }
        $testLast = ($test.GenerationNumber | ForEach-Object { [int]$_ } | Measure-Object -Maximum).Maximum
        $testEnd = @($test | Where-Object { [int]$_.GenerationNumber -eq $testLast })
        Write-Output ("Test: generation={0}, executions={1}, CEs={2:F5}, stages={3:F5}, score={4:F5}, unsorted={5:F5}" -f $testLast, $testEnd.Count, (Metric $testEnd 'AveCeLength'), (Metric $testEnd 'AveStageLength'), (Metric $testEnd 'AverageSorterPoolScore'), (Metric $testEnd 'AverageUnsortedCount'))

        $sweepName = if ($family -eq 'SimpleS' -and $workflow -eq 'Standard') { $SimpleSStandardSweep } else { 'ActivatorRates' }
        $sweep = @($eligible | Where-Object RunName -eq $sweepName)
        if ($sweep.Count -eq 0) { throw "Missing $family $workflow sweep." }
        $last = ($sweep.GenerationNumber | ForEach-Object { [int]$_ } | Measure-Object -Maximum).Maximum
        $endpoint = @($sweep | Where-Object { [int]$_.GenerationNumber -eq $last })
        $expectedMutations = @($sweep.MutationMod | Sort-Object -Unique)
        foreach ($group in ($endpoint | Group-Object $settingKey, ActivatorMutationRate | Sort-Object { Number $_.Group[0].$settingKey }, { Number $_.Group[0].ActivatorMutationRate })) {
            $samples = $group.Group
            if ($samples.Count -ne $expectedMutations.Count -or @(Compare-Object $expectedMutations @($samples.MutationMod | Sort-Object)).Count -gt 0) {
                throw "Incomplete or duplicated endpoint group: $family $workflow $($group.Name). Run the audit before comparing."
            }
            $setting = Number $samples[0].$settingKey
            $rate = Number $samples[0].ActivatorMutationRate
            $ce = Metric $samples 'AveCeLength'
            $stage = Metric $samples 'AveStageLength'
            $score = Metric $samples 'AverageSorterPoolScore'
            $range = $samples | ForEach-Object { Number $_.AverageSorterPoolScore } | Measure-Object -Minimum -Maximum
            $failedPools = @($samples | Where-Object { (Number $_.AverageUnsortedCount) -gt 0 }).Count
            Write-Output ("{0}={1}, rate={2:F2}: executions={3}, CEs={4:F5}, stages={5:F5}, score={6:F5}, scoreRange=[{7:F5},{8:F5}], poolsWithFailures={9}" -f $settingKey, $setting, $rate, $samples.Count, $ce, $stage, $score, $range.Minimum, $range.Maximum, $failedPools)
            $summaries += [pscustomobject]@{ Family=$family; Workflow=$workflow; Setting=$setting; Rate=$rate; CE=$ce; Stages=$stage; Score=$score; Failures=$failedPools }

            if ($family -eq 'SimpleS') {
                $trajectory = @($sweep | Where-Object { (Number $_.$settingKey) -eq $setting -and (Number $_.ActivatorMutationRate) -eq $rate })
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
        }
        $successful = @($summaries | Where-Object { $_.Family -eq $family -and $_.Workflow -eq $workflow -and $_.Failures -eq 0 })
        if ($successful.Count -eq 0) { throw "No fully successful $family $workflow setting." }
        $best = $successful | Sort-Object Score | Select-Object -First 1
        Write-Output ("Best observed average score: {0}={1}, rate={2:F2}, score={3:F5}, CEs={4:F5}, stages={5:F5}" -f $settingKey, $best.Setting, $best.Rate, $best.Score, $best.CE, $best.Stages)
        foreach ($setting in ($successful.Setting | Sort-Object -Unique)) {
            $bestAtSetting = $successful | Where-Object Setting -eq $setting | Sort-Object Score | Select-Object -First 1
            Write-Output ("Best rate at {0}={1}: {2:F2}, score={3:F5}" -f $settingKey, $setting, $bestAtSetting.Rate, $bestAtSetting.Score)
        }
    }
}

Write-Output "`nSimpleS versus SimpleR: best observed settings per workflow"
foreach ($workflow in @('Standard', 'Soss')) {
    $s = $summaries | Where-Object { $_.Family -eq 'SimpleS' -and $_.Workflow -eq $workflow -and $_.Failures -eq 0 } | Sort-Object Score | Select-Object -First 1
    $r = $summaries | Where-Object { $_.Family -eq 'SimpleR' -and $_.Workflow -eq $workflow -and $_.Failures -eq 0 } | Sort-Object Score | Select-Object -First 1
    Write-Output ("{0}: S p={1}, rate={2}; R AMax={3}, rate={4}; S minus R: CEs={5:F5} ({6:F2}%), stages={7:F5} ({8:F2}%), score={9:F5} ({10:F2}%)" -f $workflow, $s.Setting, $s.Rate, $r.Setting, $r.Rate, ($s.CE-$r.CE), (100*($s.CE/$r.CE-1)), ($s.Stages-$r.Stages), (100*($s.Stages/$r.Stages-1)), ($s.Score-$r.Score), (100*($s.Score/$r.Score-1)))
}
