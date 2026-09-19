module Program

open System
open System.Threading
open GeneSort.Dispatch.V1
open GeneSort.Core.Mp

MessagePackSetup.configure ()

let startTime = DateTime.Now
printfn $"**** GeneSort Engine Active: {startTime.ToString()} ****"
//DispatchSortableTest.makeParamsAndRun()
//DispatchSorterEval.makeParamsAndRun()
//DispatchSorterMutate.makeParamsAndRun()
DispatchSorterSgd.makeParamsAndRun()

let duration = DateTime.Now - startTime
Thread.Sleep(100)
printfn "********************************************"
printfn $"Total Time: {duration.ToString()}"
printfn "********************************************"
Console.ReadLine() |> ignore