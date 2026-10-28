// For more information see https://aka.ms/fsharp-console-apps

open System
open FSharp.UMX
open GeneSort.Core



module Example =
    // Helper function to print vector capabilities


    let reporter (msg:string) =
        printfn "%s: %s" (DateTime.Now.ToLongTimeString()) msg


    let runMergePermutationsExample () : permutation []  =
        let edgeLength = 4<latticeDistance>
        let dimension = 8<latticeDimension>

        let mergeLattice = MergeLattice.create dimension edgeLength
        let latticePathPermutations = MergeLattice.getPermutationsStandard (Some reporter) mergeLattice
        let result = latticePathPermutations |> LatticePathPermutations.toPermutations
        
        let boolEq = 
            result 
            |> Array.map (fun p -> Permutation.toBoolArrays p) 
            |> Array.concat
            |> Seq.distinctBy (fun arr -> System.String.Join(",", arr))
            |> Seq.toArray

        for aa in boolEq do
            printfn "%s" (System.String.Join("", aa |> Array.map (fun b -> if b then "1 " else "0 ")))

        printfn "EdgeLen: %d Dimension: %d Perm count: %d Bool Count: %d" %edgeLength dimension %result.Length boolEq.Length

        result


    let concato() =  //C:\Projects\SorterSgd.Standard_append.32p4a.Msuf\Pool_32_Test\Data\Results\Report\TextReport_SummaryReport

            let rootDir = @"C:\Projects\SorterSgd.p32.Msuf4a.SimpleS\Soss\Data\Results\Report\TextReport_SummaryReport"

            DataTableIO.concatenateAllFiles rootDir  $"{rootDir}\\MergeReport_report.txt"



    printfn "start: %s" (DateTime.Now.ToLongTimeString())

    //GenIntervalRegistry.printFirstMembers 600 100000
    concato()


    printfn "end: %s" (DateTime.Now.ToLongTimeString())





    Console.ReadLine() |> ignore
