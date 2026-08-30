module Program

open System
open System.Threading
open GeneSort.Dispatch.V1


let startTime = DateTime.Now
printfn $"**** GeneSort Engine Active: {startTime.ToString()} ****"

DispatchSorterEval.makeParamsAndRun()

let duration = DateTime.Now - startTime
Thread.Sleep(100)
printfn "********************************************"
printfn $"Total Time: {duration.ToString()}"
printfn "********************************************"
Console.ReadLine() |> ignore