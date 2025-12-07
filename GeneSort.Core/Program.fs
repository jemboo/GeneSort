// For more information see https://aka.ms/fsharp-console-apps

open GeneSort.Core.Permutation
open GeneSort.Core.PermutationSets
open GeneSort.Core
open FSharp.UMX
open System

module Example =
    let _notation p = p |> Permutation.toOrbitSet |> OrbitSet.toOrbitNotation

    let one () =
        let conjT = GeneSort.Core.Permutation.conjugate

        let p = Permutation.create [|1; 2; 0; 3|] // Permutation (0 1 2)(3)
        let q = Permutation.create [|1; 0; 3; 2|] // Permutation (0 1)(2 3)

        let r = rotated 1 4 // Rotate p by 1 position
        printfn "Permutation p: %s; %s" (p |> compose r |> _notation) (p |> compose r |>  toArrayNotation)
        printfn "Permutation q: %s; %s" (_notation q) (toArrayNotation q)
        printfn "Rotation of p by 1: %s; %s" (_notation r) (toArrayNotation r)
        printfn "Composition p ∘ q: %s; %s" (_notation (compose p q)) (toArrayNotation (compose p q))
        printfn "Conjugate q^-1 ∘ p ∘ q: %s; %s" (_notation (conjugate p q)) (toArrayNotation (conjugate p q))
        printfn "Orbits of p: %A" (_notation p)
        printfn "Powers of p: %A" (makeCyclicGroup p |> Seq.map _notation)

    // Example usage
    let two () =
        let conjT = GeneSort.Core.Permutation.conjugate

        let tc1 = Perm_Si.fromTranspositions 4 [(0, 1); (2, 3)] // Perm_Rs (0 1)(2 3)
        let tc2 = Perm_Si.adjacentTwoCycles 16 0 7
        let tc3 = Perm_Si.adjacentTwoCycles 16 1 7
        let tc4 = Perm_Si.steppedOffsetTwoCycle 16 0 2 5

        let perm = conjT tc2.Permutation tc3.Permutation
        let perm2 = conjT tc3.Permutation tc2.Permutation

        printfn "Perm_Rs (0 1)(2 3): %s" (_notation tc1.Permutation)
        printfn "evenIndexAdjacentTwoCycles 16: %s" (_notation tc2.Permutation)
        printfn "oddIndexAdjacentTwoCycles 16: %s" (_notation tc3.Permutation)
        printfn "steppedOffsetTwoCycle: %s" (_notation tc4.Permutation)

        printfn "conjugate: %s" (_notation perm)
        printfn "conjugate2: %s" (_notation perm2)


    let getBinomials() =

        let trials = 512 //25.09
        let probability = 0.01 // 50% chance of success
        
        let seed = 6290UL |> UMX.tag<randomSeed>
        let randy = new randomLcg(seed) :> IRando

        for i in 1 .. 99 do

           Console.WriteLine($"{i}: {Combinatorics.binomialSample (fun () -> randy.NextFloat()) trials probability}")

        Console.WriteLine("hi")



    let binomialHistogram (randy: IRando) (trials: int) (probability: float) (m: int) : Map<int, int> =
        if m < 1 then invalidArg "m" "Number of repetitions must be positive"
    
        let counts = 
            Seq.init m (fun _ -> Combinatorics.binomialSample (fun () -> randy.NextFloat()) trials probability)
            |> Seq.fold (fun acc successes -> 
                Map.change successes (fun count -> Some (defaultArg count 0 + 1)) acc
            ) Map.empty
    
        counts



    let printBinomialHistogramReport () : unit =

        let trials = 512 //25.09
        let probability = 0.01
        let sampleSize = 2000000
        let seed = 12902UL |> UMX.tag<randomSeed>
        let randy = new randomLcg(seed) :> IRando
        let histogram = binomialHistogram randy trials probability sampleSize

        // Calculate mean and standard deviation
        let totalSuccesses = 
            histogram 
            |> Map.fold (fun acc successes count -> acc + float successes * float count) 0.0
        let mean = totalSuccesses / float sampleSize
    
        // Print report header
        printfn "Binomial Distribution Histogram Report"
        printfn "====================================="
        printfn "Parameters:"
        printfn "  Number of trials (n): %d" trials
        printfn "  Success probability (p): %.3f" probability
        printfn "  Number of samples (m): %d" sampleSize
        printfn "  Expected mean (n*p): %.2f" (float trials * probability)
        printfn "  Expected standard deviation (sqrt(n*p*(1-p))): %.2f" (sqrt (float trials * probability * (1.0 - probability)))
        printfn ""
    
        // Print histogram
        printfn "Histogram of Successes:"
        printfn "----------------------"
        histogram
        |> Map.toSeq
        |> Seq.sortBy fst
        |> Seq.iter (fun (successes, count) ->
            let percentage = (float count / float sampleSize) * 100.0
            printfn "  %3d successes: %4d times (%.2f%%)" successes count percentage)



    let edgeLength = 128<latticeDistance>
    let dimension = 4<latticeDimension>

    let allLevelSets = LatticeLevelSetMap.getAllLevelSetMapsVV dimension edgeLength

    let zack = latticeLevelSetMap.create
                    dimension
                    edgeLength
                    (3<latticeDistance>)
                    (4<latticeDistance>)
                    LatticePoint.boundedLevelSet
                    LatticePoint.getOverCovers
                    LatticePoint.getUnderCovers

    let shuffles dex = 0

    let res = allLevelSets |> Array.map (fun llsm -> LatticeLevelSetMap.optimize llsm shuffles)

    let allGood = 
        res 
        |> Array.forall (id)

    printfn "All level sets optimized: %b" (allGood)

let wak = 5





//Example.printBinomialHistogramReport()
Console.ReadLine() |> ignore
