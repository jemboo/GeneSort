// For more information see https://aka.ms/fsharp-console-apps
open GeneSort.Project
open System

let startTime = System.DateTime.Now
printfn $"***************** {startTime.ToString()}"

WorkspaceOps.saveWorkspace FullBoolTest.workspace
FullBoolTest.RunAll()
//FullBoolTest.RunSorterEvalReport()
//RandomSorters4to64.RunAll()
//RandomSorters4n6Project.RunAll6()
//PermutationOrbitsProject.RunAll()
//PermutationOrbitsProject.RunPermuationOrbitCountReport()


let endTime = System.DateTime.Now
let duration = endTime - startTime
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

let yab = Console.ReadLine()
 