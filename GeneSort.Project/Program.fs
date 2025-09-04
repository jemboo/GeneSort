// For more information see https://aka.ms/fsharp-console-apps
open GeneSort.Project
open System

printfn $"***************** {System.DateTime.Now.ToString()}"

//Exp2.RunSorterTestCountReport()
//Exp2.RunAll()
//Exp3.RunAll()
Exp3.RunSorterEvalReport()
printfn $"****************  {System.DateTime.Now.ToString()}"

let yab = Console.ReadLine()
 