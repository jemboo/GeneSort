// For more information see https://aka.ms/fsharp-console-apps
open GeneSort.Project
open System

printfn $"***************** {System.DateTime.Now.ToString()}"

PermutationCyclesProject.RunSorterTestCountReport()
//Exp2.RunAll()
//Exp3.RunAll()
//Exp3.RunSorterEvalReport()
//Exp4.RunAll()
//Exp4.RunSorterEvalReport()
//Exp5.RunAll()
//Exp5.RunSorterEvalReport()
printfn $"**************** All done ******************"
printfn $"****************  {System.DateTime.Now.ToString()} ******************"

let yab = Console.ReadLine()
 