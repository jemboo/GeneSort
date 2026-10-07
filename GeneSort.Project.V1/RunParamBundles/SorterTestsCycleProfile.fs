namespace GeneSort.Project.V1.RunParamBundles

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.Simple.V1
open FsToolkit.ErrorHandling


[<Measure>] type partitionCount
[<Measure>] type cycleFraction


type cycleStepProfile = bool array
type cycleStep =
    private 
        { 
          profile : cycleStepProfile
          startTime: float<cycleFraction>
          endTime: float<cycleFraction>
        }


type sorterTestsCycleProfile =
    private 
        { 
          partitionCount : int<partitionCount>
          cycleSteps: cycleStep []
        }


module SorterTestsCycleProfile =

    let create (partitionCount: int<partitionCount>) (cycleSteps: cycleStep []) : Result<sorterTestsCycleProfile, string> =
        if partitionCount <= 0<partitionCount> then
            Error "Partition count must be greater than zero."
        elif Array.isEmpty cycleSteps then
            Error "Cycle steps must not be empty."
        else
            let totalTime = cycleSteps |> Array.sumBy (fun step -> step.endTime - step.startTime)
            if totalTime <> 1.0<cycleFraction> then
                Error "Total time of cycle steps must equal 1.0."
            else
                Ok { partitionCount = partitionCount; cycleSteps = Array.copy cycleSteps }