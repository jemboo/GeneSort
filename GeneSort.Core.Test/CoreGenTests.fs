namespace GeneSort.Core.Test

open Xunit
open FSharp.UMX
open GeneSort.Core
open GeneSort.Core.CollectionUtils
open GeneSort.Core.Permutation
open GeneSort.Core.PermutationSets
open GeneSort.Core.CoreGen
open System

type CoreGenTests() =

    [<Fact>]
    let ``randomPermutations generates distinct permutations`` () =
        let permutationCount = 100
        let order = 32
        let randomSeed = 123UL |> UMX.tag<randomSeed>
        let randy = new randomLcg(randomSeed);
        let perms = randomPermutations order randy 
                    |> Seq.take permutationCount
                    |> toDictionary false None (fun p -> p.Id)


        Assert.Equal(perms.Count, permutationCount)

    [<Fact>]
    let ``A permutation and it's conjugate generates the same sized cyclic group `` () =
        let permutationCount = 50
        let order = 16
        let randomSeed = 17423UL |> UMX.tag<randomSeed>
        let randy = new randomLcg(randomSeed);
        let perms = randomPermutations order randy 
                    |> Seq.take permutationCount
                    |> Seq.toArray

        let conjies = randomPermutations order randy 
                    |> Seq.take permutationCount
                    |> Seq.toArray

        for i = 0 to permutationCount - 1 do
            let perm = perms.[i]
            let permSet = makeCyclicGroup perm |> Seq.toArray
            let conj = conjugate perms.[i] conjies.[i]
            let conjSet = makeCyclicGroup conj |> Seq.toArray
            Console.WriteLine(permSet.Length)
            Assert.Equal(permSet.Length, conjSet.Length)
            
    [<Fact>]
    let ``A permutation and it's conjugate generates the same sized cyclic group `` () =
        let permutationCount = 50
        let order = 16
        let randomSeed = 17423UL |> UMX.tag<randomSeed>
        let randy = new randomLcg(randomSeed);
        let perms = randomPermutations order randy 
                    |> Seq.take permutationCount
                    |> Seq.toArray

        let conjies = randomPermutations order randy 
                    |> Seq.take permutationCount
                    |> Seq.toArray

        for i = 0 to permutationCount - 1 do
            let perm = perms.[i]
            let permSet = makeCyclicGroup perm |> Seq.toArray
            let conj = conjugate perms.[i] conjies.[i]
            let conjSet = makeCyclicGroup conj |> Seq.toArray
            Console.WriteLine(permSet.Length)
            Assert.Equal(permSet.Length, conjSet.Length)
            

            
    [<Fact>]
    let ``Histogram of permutation group length`` () =
        let permutationCount = 100
        let order = 16
        let randomSeed = 17423UL |> UMX.tag<randomSeed>
        let randy = new randomLcg(randomSeed)
        let perms = randomPermutations order randy 
                    |> Seq.take permutationCount
                    |> Seq.toArray


        // Collect lengths of permSet
        let lengths = 
            [| for i = 0 to permutationCount - 1 do
                let perm = perms.[i]
                let permSet = makeCyclicGroup perm |> Seq.toArray
                yield permSet.Length |]

        // Create histogram data
        let histogram = 
            lengths
            |> Array.groupBy id
            |> Array.map (fun (len, arr) -> len, arr.Length)
            |> Array.sortBy fst
            |> Array.map (fun (len, count) -> len, count, String.replicate (count / 10) "*") // Scale for display

        // Print histogram
        System.Diagnostics.Debug.WriteLine("\nHistogram of permSet.Length:")
        System.Diagnostics.Debug.WriteLine("Length | Count | Visual")
        System.Diagnostics.Debug.WriteLine("-------|-------|-------")
        for (len, count, bars) in histogram do
            System.Diagnostics.Debug.WriteLine($"{len,-6} | {count,-5} | {bars}")
    
        Assert.Equal(1, 1)


    [<Fact>]
    let ``Cycles of identity permutation returns only identity`` () =
        let id = identity 4
        let cycles = makeCyclicGroup id |> Seq.toList
        let expected = [identity 4]
        Assert.Equal<int array list>(List.map (fun (p: permutation) -> p.Array) expected, 
                                     List.map (fun (p: permutation) -> p.Array) cycles)


    [<Fact>]
    let ``The conjugate of a Perm_Rs is a Perm_Rs`` () =
        let testCount = 50
        let order = 16
        let randomSeed = 17423UL |> UMX.tag<randomSeed>
        let randy = new randomLcg(randomSeed);
        let perm_Rss = randomSaturatedPerm_Si order randy 
                        |> Seq.take testCount
                        |> Seq.toArray

        let conjies = randomPermutations order randy 
                    |> Seq.take testCount
                    |> Seq.toArray

        for i = 0 to testCount - 1 do
            let conj = Perm_Si.conjugate perm_Rss.[i] conjies.[i]
            Assert.True (conj.Permutation |> Permutation.isSelfInverse)
            