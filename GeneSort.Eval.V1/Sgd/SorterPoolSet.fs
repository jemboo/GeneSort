namespace GeneSort.Eval.V1

open System
open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1

type sorterPoolSet =
    private {
        _sorterPoolSetId: Guid<sorterPoolSetId>
        _sorterPools: Map<Guid<sorterPoolId>, sorterPool>
        _generationNumber: int<generationNumber>
    }
    member this.SorterPoolSetId = this._sorterPoolSetId
    member this.SorterPools = this._sorterPools
    member this.GenerationNumber = this._generationNumber

    static member Create(sorterPoolSetId, generationNumber, ?pools: seq<sorterPool>) =
        let poolsMap = 
            defaultArg pools Seq.empty
            |> Seq.map (fun p -> p.SorterPoolId, p)
            |> Map.ofSeq
        { 
            _sorterPoolSetId = sorterPoolSetId
            _sorterPools = poolsMap 
            _generationNumber = generationNumber 
        }


module SorterPoolSet =

    /// Safely attempts to find a specific SorterPool within the set
    let tryFindPool (poolId: Guid<sorterPoolId>) (poolSet: sorterPoolSet) : sorterPool option =
        Map.tryFind poolId poolSet._sorterPools

    /// Adds or updates a SorterPool inside the SorterPoolSet
    let upsertPool (pool: sorterPool) (poolSet: sorterPoolSet) : sorterPoolSet =
        let updatedMap = Map.add pool.SorterPoolId pool poolSet._sorterPools
        { poolSet with _sorterPools = updatedMap }

    /// Advances the generation counter by a given step count
    let advanceGeneration (steps: int) (poolSet: sorterPoolSet) : sorterPoolSet =
        { poolSet with _generationNumber = (%poolSet._generationNumber + steps) |> UMX.tag }

    /// Mutates every single pool across the entire pool set uniformly
    let mutate 
            (sorterModelMutator: sorterModelMutator) 
            (mutantsPerSorter: int<sorterCount>)  
            (poolSet: sorterPoolSet) : sorterPoolSet =
        
        let mutatedPools = 
            poolSet._sorterPools 
            |> Map.map (fun _ pool -> SorterPool.mutate sorterModelMutator mutantsPerSorter pool)

        { poolSet with _sorterPools = mutatedPools }

    /// Extracts all evaluations across all members of all pools into a single flat map
    let extractSorterEvals (poolSet: sorterPoolSet) : Map<Guid<sorterPoolMemberId>, sorterEval> =
        poolSet._sorterPools
        |> Map.values
        |> Seq.map SorterPool.extractSorterEvals
        |> Seq.fold (fun accMap individualPoolMap -> 
            // Merge maps cleanly
            Map.fold (fun acc key value -> Map.add key value acc) accMap individualPoolMap
        ) Map.empty

    /// Updates the evaluations of individual pool members across all relevant sub-pools.
    /// The resulting pool set will only preserve pool members actively found in the evaluation map.
    let updateSorterPools 
                (evalMap: Map<Guid<sorterPoolMemberId>, sorterEval>) 
                (poolSet: sorterPoolSet) : sorterPoolSet =
        let updatedPools = 
            poolSet._sorterPools
            |> Map.map (fun _ pool -> SorterPool.updateSorterPool evalMap pool)

        { poolSet with _sorterPools = updatedPools }

    /// Trims every pool inside the set down to the designated pruned size using the given evaluation rule
    let pruneSorterPools 
                (measure: sorterEvalMeasure) 
                (prunedSize: int<sorterCount>) 
                (poolSet: sorterPoolSet) : sorterPoolSet =
        
        let prunedPools = 
            poolSet._sorterPools
            |> Map.map (fun _ pool -> SorterPool.pruneSorterPool pool measure prunedSize)

        { poolSet with _sorterPools = prunedPools }



//let nextGeneration = 
//    currentPoolSet
//    |> SorterPoolSet.mutate mutator 2<sorterCount>
//    // ... evaluate members here ...
//    |> SorterPoolSet.updateSorterPools computedEvals
//    |> SorterPoolSet.pruneSorterPools selectionMeasure 10<sorterCount>
//    |> SorterPoolSet.advanceGeneration 1