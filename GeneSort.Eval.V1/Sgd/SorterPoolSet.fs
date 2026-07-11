namespace GeneSort.Eval.V1.Sgd

open System
open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Eval.V1


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
            (mutantsPerSorter: int<sorterChildCount>)  
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
    let updateSorterEvals 
                (evalMap: Map<Guid<sorterPoolMemberId>, sorterEval>) 
                (poolSet: sorterPoolSet) : sorterPoolSet =
        let updatedPools = 
            poolSet._sorterPools
            |> Map.map (fun _ pool -> SorterPool.updateSorterEval evalMap pool)

        { poolSet with _sorterPools = updatedPools }

    /// Trims every pool inside the set down to the designated pruned size using the given evaluation rule
    let pruneSorterPools 
                (measure: sorterEvalMeasure)
                (prioritizeNewMutants: bool<prioritizeNewMutants>)
                (distinctSorterHashes: bool<distinctSorterHashes>)
                (sorterCountPerPool: int<sorterCountPerPool>) 
                (poolSet: sorterPoolSet) : sorterPoolSet =
        
        let prunedPools = 
            poolSet._sorterPools
            |> Map.map (fun _ pool -> SorterPool.pruneSorterPool 
                                            pool 
                                            measure
                                            prioritizeNewMutants
                                            distinctSorterHashes 
                                            sorterCountPerPool)

        { poolSet with _sorterPools = prunedPools }



    /// Initializes a sorterPoolSet with poolCount pools from a sorterModelSet, each pool
    /// having sortersPerPool members. Takes the first (poolCount * sortersPerPool) sorters
    /// from sorterPool, and throws if there are not enough.
    let fromSorterModelSet 
            (sorterPoolSetId: Guid<sorterPoolSetId>) 
            (poolCount: int<sorterPoolCount>)
            (sortersPerPool: int<sorterCountPerPool>)
            (generationNumber: int<generationNumber>)
            (evalLabelMap: Map<Guid<sorterModelId>, evalLabel>)
            (modelSet: sorterModelSet) : sorterPoolSet =

        let totalRequiredSorters = %poolCount * %sortersPerPool
        let availableModels = modelSet.SorterModels

        // Guard: Verify the source model set contains enough elements to completely satisfy the requested layout bounds
        if availableModels.Length < totalRequiredSorters then
            raise (ArgumentException(
                sprintf "Insufficient models in sorterModelSet. Required: %d (Pools: %d, SortersPerPool: %d), Available: %d." 
                    totalRequiredSorters %poolCount %sortersPerPool availableModels.Length))

        // 1. Slice the exact number of required parent sorter models from the array head
        let targetedModels = availableModels |> Array.take totalRequiredSorters

        // 2. Fragment the contiguous models stream into distinct array slices per pool block boundary
        let pools = 
            targetedModels
            |> Array.chunkBySize %sortersPerPool
            |> Array.map (fun modelChunk ->
                
                let poolName =
                    modelChunk |> Array.map (fun model -> 
                        let id = model |> SorterModel.getId
                        Map.find id evalLabelMap
                    ) |> EvalLabel.toString |> UMX.tag<sorterPoolName>

                // 3. Map each model within the chunk into a tracking pool member record
                let sorterPoolMembers = 
                    modelChunk
                    |> Array.map (fun model ->
                        let poolMemberId = Guid.NewGuid() |> UMX.tag<sorterPoolMemberId>
                        sorterPoolMember.Create
                            poolMemberId
                            model
                            (0 |> UMX.tag<mutationIndex>)   // Initial tracking index starts at 0
                            None                            // Root node element has no parent mutation source
                            None                            // Transient evaluation state begins unassigned
                    )

                // 4. Wrap the evaluated array segment block inside an explicit tracking pool object container
                let poolId = Guid.NewGuid() |> UMX.tag<sorterPoolId>
                sorterPool.create poolId poolName sorterPoolMembers modelSet.MaxCeLength
            )

        // 5. Package the complete group layout structure back out to the main generational repository root
        sorterPoolSet.Create(sorterPoolSetId, generationNumber, pools)



    let toDataTableRecords (sorterPoolSet: sorterPoolSet) : dataTableRecord [] =
        [||]
