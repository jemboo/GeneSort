namespace GeneSort.Eval.V1

open System
open FSharp.UMX
open GeneSort.Model.Sorting
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1


type sorterPool =
    private {
        _sorterPoolId: Guid<sorterPoolId>
        _sorterPoolMembers: Map<Guid<sorterPoolMemberId>, sorterPoolMember>
    }

    member this.SorterPoolId = this._sorterPoolId

    // Exposed sequence directly from the map values for efficiency
    member this.SorterPoolMembers : sorterPoolMember seq =
        Map.values this._sorterPoolMembers


    static member Create(sorterPoolId, ?members: seq<sorterPoolMember>) =
        let membersMap = 
            defaultArg members Seq.empty
            |> Seq.map (fun m -> m.SorterPoolMemberId, m)
            |> Map.ofSeq
        { 
            _sorterPoolId = sorterPoolId
            _sorterPoolMembers = membersMap
        }


module SorterPool = 

    /// Adds or updates a member inside the pool
    let upsertMember (memberToUpsert: sorterPoolMember) (pool: sorterPool) : sorterPool =
        let updatedMap = Map.add memberToUpsert.SorterPoolMemberId memberToUpsert pool._sorterPoolMembers
        { pool with _sorterPoolMembers = updatedMap }

    /// Finds a member and updates its evaluation within the pool context
    let updateMemberEval (memberId: Guid<sorterPoolMemberId>) (eval: sorterEval option) (pool: sorterPool) : sorterPool =
        match Map.tryFind memberId pool._sorterPoolMembers with
        | Some memberObj ->
            let updatedMember = memberObj |> SorterPoolMember.withEval eval
            upsertMember updatedMember pool
        | None -> pool


    /// The returned SorterPool only contains members with sorterPoolMemberIds that are found in map
    let updateSorterPool (map: Map<Guid<sorterPoolMemberId>, sorterEval>) (pool: sorterPool) : sorterPool =
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
        pool._sorterPoolMembers
        |> Map.values
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
            (mutantsPerSorter: int<sorterCount>)  
            (pool: sorterPool) : sorterPool =

        let updatedMembersMap =
            pool._sorterPoolMembers
            |> Map.values
            |> Seq.fold (fun accMap currentMember ->
                // Invoke the member-level mutation strategy designed earlier
                let updatedParent, childMutants = 
                    SorterPoolMember.mutate sorterModelMutator currentMember mutantsPerSorter
                
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
                (prunedSize: int<sorterCount>) : sorterPool =
        
        let targetSize = max 0 %prunedSize
        let scoreFunc = SorterEvalFunctions.getFunctionForMeasure measure
        let filterUnsorted = SorterEvalFunctions.getFilterUnsortedFlag measure

        let sortedSurvivors =
            pool._sorterPoolMembers
            |> Map.values
            // Step 1: Handle filtering of unsorted elements if required by the measure rules
            |> Seq.filter (fun spm ->
                if filterUnsorted then
                    match spm.SorterEval with
                    | Some eval -> SorterEval.getUnsortedCount eval <= 0<sortableCount>
                    | None -> false // Unevaluated members cannot verify if they are fully sorted
                else true
            )
            // Step 2: Score members. Lower scores are better (fewer CEs, fewer stages, less unsorted).
            // We map into a sortable tuple: (Score, PoolMember)
            // Unevaluated members (None) are treated as Double.PositiveInfinity (worst possible score)
            |> Seq.map (fun spm ->
                match spm.SorterEval with
                | Some eval -> (scoreFunc eval, spm)
                | None -> (Double.PositiveInfinity, spm)
            )
            // Step 3: Sort ascending (best scores first)
            |> Seq.sortBy fst
            // Step 4: Take the best up to the designated pruned size limit
            |> Seq.truncate targetSize
            |> Seq.map snd

        // Rebuild into a clean SorterPool record format
        sorterPool.Create(pool.SorterPoolId, sortedSurvivors)

