// For more information see https://aka.ms/fsharp-console-apps
open GeneSort.Project
open System

let startTime = System.DateTime.Now
printfn $"**** QQQ ******** {startTime.ToString()}"

//WorkspaceOps.saveWorkspace FullBoolTest.workspace
//FullBoolTest.RunAll()
//FullBoolTest.RunSorterEvalReport()


WorkspaceOps.saveWorkspace MergeIntTests.workspace
//MergeIntTests.RunAll()
MergeIntTests.RunSorterEvalReport()


let endTime = System.DateTime.Now
let duration = endTime - startTime
printfn $"**************** All done ******************"
printfn $"****************  {duration.ToString()} ******************"

let yab = Console.ReadLine()
 