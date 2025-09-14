// For more information see https://aka.ms/fsharp-console-apps
open GeneSort.Project
open System

printfn $"***************** {System.DateTime.Now.ToString()}"


//RandomSortersProject.RunAll4()
RandomSortersProject.RunAll6()
//PermutationOrbitsProject.RunAll()
//PermutationOrbitsProject.RunPermuationOrbitCountReport()

printfn $"**************** All done ******************"
printfn $"****************  {System.DateTime.Now.ToString()} ******************"

let yab = Console.ReadLine()
 