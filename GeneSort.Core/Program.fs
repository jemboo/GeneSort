// For more information see https://aka.ms/fsharp-console-apps

open GeneSort.Core
open FSharp.UMX
open System



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



    let yab = MathUtils.expSampler 50000 1.0068

    printfn "start: %s" (DateTime.Now.ToLongTimeString())

    let rootDir = @"C:\Projects\SorterSgd.Msce\Rmu\Data\Report\TextReport_SorterRunResult_report"
   // let rootDir = @"C:\Projects\SorterSgd.Msce\Rsu\Data\Report\TextReport_SorterRunResult_report"
    //let rootDir = @"C:\Projects\SorterMutate.Msce\Rsu\Data\Report\TextReport_MutantReport"
    DataTableIO.concatenateAllFiles 
                rootDir
                $"{rootDir}\\MergeReport_report.txt"


    printfn "end: %s" (DateTime.Now.ToLongTimeString())





Console.ReadLine() |> ignore
