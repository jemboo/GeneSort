namespace GeneSort.Eval.V1.Sgd

open System
open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1


type sorterPool =
    private {
        _name: string<sorterPoolName>
        _sorterPoolId: Guid<sorterPoolId>
        _sorterPoolMembers: Map<Guid<sorterPoolMemberId>, sorterPoolMember>
        _rawCeLength: int<ceLength>
    }
    member this.RawCeLength with get() = this._rawCeLength
    member this.Name with get() = this._name
    member this.SorterPoolMembers with get() :sorterPoolMember seq =
        Map.values this._sorterPoolMembers
    member this.SorterPoolId with get() = this._sorterPoolId


    static member create 
            (sorterPoolId: Guid<sorterPoolId>) 
            (name: string<sorterPoolName>)
            (members: sorterPoolMember []) 
            (rawCeLength: int<ceLength>) =
        let membersMap = 
            members
            |> Seq.map (fun m -> m.SorterPoolMemberId, m)
            |> Map.ofSeq
        { 
            _name = name
            _sorterPoolId = sorterPoolId
            _sorterPoolMembers = membersMap
            _rawCeLength = rawCeLength
        }


module SorterPool = 

    /// Adds or updates a member inside the pool
    let upsertMember 
            (memberToUpsert: sorterPoolMember) 
            (pool: sorterPool) : sorterPool =
        let updatedMap = Map.add 
                            memberToUpsert.SorterPoolMemberId 
                            memberToUpsert 
                            pool._sorterPoolMembers
        { pool with _sorterPoolMembers = updatedMap }

    /// Finds a member and updates its evaluation within the pool context
    let updateMemberEval 
                    (memberId: Guid<sorterPoolMemberId>) 
                    (eval: sorterEval option) 
                    (pool: sorterPool) : sorterPool =
        match Map.tryFind memberId pool._sorterPoolMembers with
        | Some memberObj ->
            let updatedMember = memberObj |> SorterPoolMember.withEval eval
            upsertMember updatedMember pool
        | None -> pool


    /// The returned SorterPool only contains members with sorterPoolMemberIds that are found in map
    let updateSorterEval (map: Map<Guid<sorterPoolMemberId>, sorterEval>) (pool: sorterPool) : sorterPool =
        let updatedMembersMap =
            map 
            |> Map.fold (fun acc poolMemberId eval ->
                match Map.tryFind poolMemberId pool._sorterPoolMembers with
                | Some memberObj ->
                    // Update the evaluation and accumulate it into the new map
                    let updatedMember = memberObj |> SorterPoolMember.withEval (Some eval)
                    Map.add poolMemberId updatedMember acc
                | None -> 
                    // If it's in the map but not in the pool, it is ignored
                    acc
            ) Map.empty

        { pool with _sorterPoolMembers = updatedMembersMap }


    /// Gets the sorterEvals from the sorterPool, and ignores SorterPoolMembers that don't have them
    let extractSorterEvals (pool: sorterPool) : Map<Guid<sorterPoolMemberId>, sorterEval> =
        pool.SorterPoolMembers
        |> Seq.fold (fun accMap spm ->
            match spm.SorterEval with
            | Some eval -> 
                Map.add spm.SorterPoolMemberId eval accMap
            | None -> 
                // Ignore members that don't have an evaluation yet
                accMap
        ) Map.empty


    /// Applies the same mutantsPerSorter count to every pool member, accumulating 
    /// the advanced parents and all newly spawned mutants into a single updated pool.
    let mutate 
            (sorterModelMutator: sorterModelMutator) 
            (mutantsPerSorter: int<sorterChildCount>)  
            (currentGeneration: int<generationNumber>)
            (pool: sorterPool) : sorterPool =

        let updatedMembersMap =
            pool.SorterPoolMembers
            |> Seq.fold (fun accMap currentMember ->
                // Invoke the member-level mutation strategy designed earlier
                let updatedParent, childMutants = 
                    SorterPoolMember.mutate 
                            sorterModelMutator 
                            currentMember 
                            mutantsPerSorter
                            currentGeneration

                // Add the updated parent to our accumulator map
                let mapWithParent = Map.add updatedParent.SorterPoolMemberId updatedParent accMap
                
                // Add all newly created child mutants to the accumulator map
                childMutants 
                |> Array.fold (fun mapAcc child -> 
                    Map.add child.SorterPoolMemberId child mapAcc
                ) mapWithParent

            ) Map.empty

        { pool with _sorterPoolMembers = updatedMembersMap }


    /// Trims the SorterPool to size prunedSize, selecting the best (lowest score) according to measure
    let pruneSorterPool 
                (pool: sorterPool) 
                (measure: sorterEvalMeasure) 
                (prioritizeNewMutants: bool<prioritizeNewMutants>)
                (distinctSorterHashes: bool<distinctSorterHashes>)
                (sorterCountPerPool: int<sorterCountPerPool>) : sorterPool =
        
        let targetSize = max 0 %sorterCountPerPool
        let scoreFunc = SorterEvalFunctions.getFunctionForMeasure measure
        let filterUnsorted = SorterEvalFunctions.getFilterUnsortedFlag measure

        let filter1 =
            pool.SorterPoolMembers
            // Step 1: Handle filtering of unsorted elements if required by the measure rules
            |> Seq.filter (fun spm ->
                if filterUnsorted then
                    match spm.SorterEval with
                    | Some eval -> SorterEval.getUnsortedCount eval <= 0<sortableCount>
                    | None -> false // Unevaluated members cannot verify if they are fully sorted
                else true
            )

        let birthdaySort = 
            if %prioritizeNewMutants then
                filter1 |> Seq.sortBy(fun spm -> - %spm.Birthday)
            else
                filter1 |> Seq.sortBy(fun spm -> spm.Birthday )

        let filter2 =
            if %distinctSorterHashes then
                birthdaySort 
                |> Seq.distinctBy (fun spm -> %(SorterEval.getSequenceHash spm.SorterEval.Value))
            else
                birthdaySort

        let sortedSurvivors =
            filter2
            // Step 2: Score members and construct the sorting key matrix
            // Unevaluated members (None) get Double.PositiveInfinity (worst possible score)
            |> Seq.map (fun spm ->
                let score = 
                    match spm.SorterEval with
                    | Some eval -> scoreFunc eval
                    | None -> Double.PositiveInfinity
                (score, spm)
            )
            // Step 3: Sort ascending (best scores first). 
            // Tie-break on MutationIndex when scores match uniformly.
            |> Seq.sortBy (fun (score, spm) ->
                let mIndexRaw = %spm.MutationIndex
                
                // If prioritizing NEW mutants: lower mutation index comes first.
                // If prioritizing OLD members: higher mutation index comes first (so we negate it).
                let tieBreaker = if %prioritizeNewMutants then mIndexRaw else -mIndexRaw
                
                (score, tieBreaker)
            )
            // Step 4: Take the best up to the designated pruned size limit
            |> Seq.truncate targetSize
            |> Seq.map snd
            |> Seq.toArray

        sorterPool.create pool.SorterPoolId pool.Name sortedSurvivors pool.RawCeLength





















    ///// Trims the SorterPool to size prunedSize, selecting the best (lowest score) according to measure
    //let pruneSorterPool 
    //            (pool: sorterPool) 
    //            (measure: sorterEvalMeasure) 
    //            (prioritizeNewMutants: bool<prioritizeNewMutants>)
    //            (distinctSorterHashes: bool<distinctSorterHashes>)
    //            (sorterCountPerPool: int<sorterCountPerPool>) : sorterPool =
        
    //    let targetSize = max 0 %sorterCountPerPool
    //    let scoreFunc = SorterEvalFunctions.getFunctionForMeasure measure
    //    let filterUnsorted = SorterEvalFunctions.getFilterUnsortedFlag measure

    //    let filter1 =
    //        pool._sorterPoolMembers
    //        |> Map.values
    //        // Step 1: Handle filtering of unsorted elements if required by the measure rules
    //        |> Seq.filter (fun spm ->
    //            if filterUnsorted then
    //                match spm.SorterEval with
    //                | Some eval -> SorterEval.getUnsortedCount eval <= 0<sortableCount>
    //                | None -> false // Unevaluated members cannot verify if they are fully sorted
    //            else true
    //        )

    //    let filter2 =
    //        if %distinctSorterHashes then
    //            filter1 |> Seq.distinctBy (fun spm -> %(SorterEval.getSequenceHash spm.SorterEval.Value))
    //        else
    //            filter1

    //    let sortedSurvivors =
    //        filter2
    //        // Step 2: Score members and construct the sorting key matrix
    //        // Unevaluated members (None) get Double.PositiveInfinity (worst possible score)
    //        |> Seq.map (fun spm ->
    //            let score = 
    //                match spm.SorterEval with
    //                | Some eval -> scoreFunc eval
    //                | None -> Double.PositiveInfinity
    //            (score, spm)
    //        )
    //        // Step 3: Sort ascending (best scores first). 
    //        // Tie-break on MutationIndex when scores match uniformly.
    //        |> Seq.sortBy (fun (score, spm) ->
    //            let mIndexRaw = %spm.MutationIndex
                
    //            // If prioritizing NEW mutants: lower mutation index comes first.
    //            // If prioritizing OLD members: higher mutation index comes first (so we negate it).
    //            let tieBreaker = if %prioritizeNewMutants then mIndexRaw else -mIndexRaw
                
    //            (score, tieBreaker)
    //        )
    //        // Step 4: Take the best up to the designated pruned size limit
    //        |> Seq.truncate targetSize
    //        |> Seq.map snd
    //        |> Seq.toArray

    //    sorterPool.create pool.SorterPoolId pool.Name sortedSurvivors pool.RawCeLength

