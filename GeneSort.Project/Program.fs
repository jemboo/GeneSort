// For more information see https://aka.ms/fsharp-console-apps
open GeneSort.Project
open System

printfn $"***************** {System.DateTime.Now.ToString()}"

WorkspaceOps.saveWorkspace FullBoolTest.workspace
//FullBoolTest.RunAll()
FullBoolTest.RunSorterEvalReport()
//RandomSorters4to64.RunAll()
//RandomSorters4n6Project.RunAll6()
//PermutationOrbitsProject.RunAll()
//PermutationOrbitsProject.RunPermuationOrbitCountReport()

printfn $"**************** All done ******************"
printfn $"****************  {System.DateTime.Now.ToString()} ******************"

let yab = Console.ReadLine()
 