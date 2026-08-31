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
        _latticeBounds: latticeBounds
    }
    member this.SorterPoolSetId with get() = this._sorterPoolSetId
    member this.SorterPools with get() = this._sorterPools
    member this.SorterPoolCount with get() = this._sorterPools.Count |> UMX.tag<sorterPoolCount>
    member this.GenerationNumber with get() = this._generationNumber
    member this.LatticeBounds with get() = this._latticeBounds

    static member create (sorterPoolSetId: Guid<sorterPoolSetId>)
                         (generationNumber: int<generationNumber>)
                         (bounds: latticeBounds)
                         (pools: seq<sorterPool> option) =
        let poolsMap = 
            defaultArg pools Seq.empty
            |> Seq.map (fun p -> p.SorterPoolId, p)
            |> Map.ofSeq
        { 
            _sorterPoolSetId = sorterPoolSetId
            _sorterPools = poolsMap 
            _generationNumber = generationNumber 
            _latticeBounds = bounds
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


    // reduces the sorterPoolCount by a factor of sorterPoolExpansionRate, effectively pruning the pool set,
    // selecting only the top-performing pools based on SorterPool.getAverageScore
    let trimPools (sorterPoolExpansionRate: int<sorterPoolExpansionRate>) 
                  (measure: sorterPoolMeasure) 
                  (poolSet: sorterPoolSet) : sorterPoolSet =

        let currentPoolCount = poolSet._sorterPools.Count
        if currentPoolCount = 0 then
            poolSet
        else
            let expansionFactor = %sorterPoolExpansionRate

            // Guard: Expansion factor must be positive and non-zero to avoid division errors
            if expansionFactor <= 0 then
                raise (ArgumentException(
                    sprintf "sorterPoolExpansionRate must be greater than 0, but was %d." expansionFactor))

            // Guard: Pool count must be evenly divisible by sorterPoolExpansionRate
            if currentPoolCount % expansionFactor <> 0 then
                raise (ArgumentException(
                    sprintf "Current pool count (%d) is not divisible by sorterPoolExpansionRate (%d)." 
                        currentPoolCount expansionFactor))

            let targetCount = currentPoolCount / expansionFactor

            let updatedPools =
                poolSet._sorterPools
                |> Map.values
                |> Seq.map (fun pool -> 
                    let score = PoolEvalFunctions.getPoolScore measure pool
                    (score, pool)
                )
                // Lower score represents better performance; sort ascending
                |> Seq.sortBy (fun (avgScore, _) -> %avgScore)
                |> Seq.truncate targetCount
                |> Seq.map snd

            sorterPoolSet.create poolSet.SorterPoolSetId poolSet.GenerationNumber poolSet.LatticeBounds (Some updatedPools)


    // Increases the poolSet.PoolCount by a factor of sorterPoolExpansionRate.
    // Assigns distinct mutationMod values [0 .. (sorterPoolExpansionRate - 1)] to each new pool
    let expandPools (sorterPoolExpansionRate: int<sorterPoolExpansionRate>) 
                        (poolSet: sorterPoolSet) : sorterPoolSet =

            let expansionFactor = %sorterPoolExpansionRate

            if expansionFactor <= 0 then
                raise (ArgumentException(
                    sprintf "sorterPoolExpansionRate must be greater than 0, but was %d." expansionFactor))

            if Map.isEmpty poolSet._sorterPools then
                poolSet
            else
                let expandedPools =
                    poolSet._sorterPools
                    |> Map.values
                    |> Seq.collect (fun parentPool ->
                        Array.init expansionFactor (fun modValue ->
                            let newPoolId = Guid.NewGuid() |> UMX.tag<sorterPoolId>
                            let newMutationMod = modValue |> UMX.tag<mutationMod>
                        
                            SorterPool.deriveChildPool newPoolId newMutationMod parentPool
                        )
                    )

                sorterPoolSet.create poolSet.SorterPoolSetId poolSet.GenerationNumber poolSet.LatticeBounds (Some expandedPools)


    /// Mutates every single pool across the entire pool set uniformly
    let mutate 
            (sorterModelMut: sorterModelMutator) 
            (mutantsPerSorter: int<sorterChildCount>)  
            (poolSet: sorterPoolSet): sorterPoolSet =
        
        let mutatedPools = 
            poolSet._sorterPools 
            |> Map.map (fun _ pool -> SorterPool.mutate 
                                            sorterModelMut 
                                            mutantsPerSorter
                                            poolSet.GenerationNumber
                                            pool)

        { poolSet with _sorterPools = mutatedPools }

    // mutates all the members of the poolSet
    // only keeps selectedSorterCountPerPool of each of the original pool members,
    // prioritized according to selectionMeasure
    let mutateAndTrim
            (sorterModelMut: sorterModelMutator)
            (selectedSorterCountPerPool: int<sorterCountPerPool>)
            (selectionMeasure: sorterEvalMeasure)
            (mutantsPerSorter: int<sorterChildCount>)  
            (poolSet: sorterPoolSet): sorterPoolSet =
        
        let mutatedPools = 
            poolSet._sorterPools 
            |> Map.map (fun _ pool -> SorterPool.mutateAndTrim 
                                            sorterModelMut 
                                            selectedSorterCountPerPool
                                            selectionMeasure
                                            mutantsPerSorter
                                            poolSet.GenerationNumber
                                            pool)

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
            |> Map.map (fun _ pool -> SorterPool.pruneSorterPoolDebug 
                                            pool 
                                            measure
                                            prioritizeNewMutants
                                            distinctSorterHashes 
                                            sorterCountPerPool)

        { poolSet with _sorterPools = prunedPools }

    /// Iterates through all sorter pools in the set and adjusts their RawCeLengths
    /// and member population based on sortedFractionThreshold.
    let adjustCeLengths
            (sortedFractionThreshold: float<sortedFraction>)
            (poolSet: sorterPoolSet) : sorterPoolSet =

        let updatedPools =
            poolSet.SorterPools
            |> Map.toSeq
            |> Seq.map (fun (_, pool) ->
                SorterPool.adjustCeLengthByThreshold sortedFractionThreshold pool
            )
            |> Seq.toArray

        sorterPoolSet.create poolSet.SorterPoolSetId poolSet.GenerationNumber poolSet.LatticeBounds (Some (updatedPools :> seq<_>))


    /// Initializes a sorterPoolSet with poolCount pools from a sorterModelSet, each pool
    /// having sortersPerPool members. Takes the first (poolCount * sortersPerPool) sorters
    /// from sorterPool, and throws if there are not enough.
    let fromSorterModelSet 
            (sorterPoolSetId: Guid<sorterPoolSetId>) 
            (poolCount: int<sorterPoolCount>)
            (sortersPerPool: int<sorterCountPerPool>)
            (generationNumber: int<generationNumber>)
            (evalLabelMap: Map<Guid<sorterModelId>, evalLabel>)
            (modelSet: sorterModelSet) 
            (mutationMod: int<mutationMod>) 
            (bounds: latticeBounds): sorterPoolSet =

        // 1. Guard check: latticeBounds volume must equal requested poolCount
        let expectedPoolCount = SorterPoolTag.totalCells bounds
        if %poolCount <> expectedPoolCount then
            invalidArg (nameof poolCount) (sprintf "poolCount (%d) must match latticeBounds volume (%d)." %poolCount expectedPoolCount)

        let totalRequiredSorters = %poolCount * %sortersPerPool
        let availableModels = modelSet.SorterModels

        //if availableModels.Length < totalRequiredSorters then
        //    invalidArg (nameof modelSet) (sprintf "Model set only contains %d models, but %d are required." availableModels.Length totalRequiredSorters)

        // 2. Slice the exact number of required parent sorter models from the array head
        let targetedModels = availableModels |> Array.truncate totalRequiredSorters
        let adjSortersPerPool = ((float targetedModels.Length) / (float %poolCount)) |> floor |> int

        // 3. Fragment models into pools and assign tag via SorterPoolTag.fromIndex
        let pools = 
            targetedModels
            |> Array.chunkBySize adjSortersPerPool
            |> Array.mapi (fun dex modelChunk ->
                
                let poolName =
                    modelChunk |> Array.map (fun model -> 
                        let id = model |> SorterModel.getId
                        Map.find id evalLabelMap
                    ) |> EvalLabel.toString |> UMX.tag<sorterPoolName>

                let sorterPoolMembers = 
                    modelChunk
                    |> Array.map (fun model ->
                        let poolMemberId = Guid.NewGuid() |> UMX.tag<sorterPoolMemberId>
                        sorterPoolMember.create
                            poolMemberId
                            model
                            (0 |> UMX.tag<mutationIndex>)
                            mutationMod
                            None
                            None
                            0<generationNumber>
                    )

                let poolId = Guid.NewGuid() |> UMX.tag<sorterPoolId>
                let tag = SorterPoolTag.fromIndex bounds dex

                sorterPool.create 
                        poolId 
                        None
                        poolName
                        tag
                        sorterPoolMembers 
                        modelSet.RawCeLength 
                        mutationMod
            )

        // 4. Package into updated curried constructor
        sorterPoolSet.create sorterPoolSetId generationNumber bounds (Some (pools :> seq<_>))

