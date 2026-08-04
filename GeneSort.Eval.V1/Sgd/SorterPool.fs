namespace GeneSort.Eval.V1.Sgd

open System
open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Eval.V1
open System.Diagnostics


type sorterPool =
    private {
        _name: string<sorterPoolName>
        _sorterPoolId: Guid<sorterPoolId>
        _sorterPoolMembers: Map<Guid<sorterPoolMemberId>, sorterPoolMember>
        _rawCeLength: int<ceLength>
        _mutationMod: int<mutationMod>
    }

    member this.MutationMod with get() = this._mutationMod
    member this.Name with get() = this._name
    member this.RawCeLength with get() = this._rawCeLength
    member this.SorterPoolMembers with get() :sorterPoolMember seq =
        Map.values this._sorterPoolMembers
    member this.SorterPoolId with get() = this._sorterPoolId

    static member create 
            (sorterPoolId: Guid<sorterPoolId>) 
            (name: string<sorterPoolName>)
            (members: sorterPoolMember []) 
            (rawCeLength: int<ceLength>) 
            (mutationMod: int<mutationMod>) =
        let membersMap = 
            members
            |> Seq.map (fun m -> m.SorterPoolMemberId, m)
            |> Map.ofSeq
        { 
            _name = name
            _sorterPoolId = sorterPoolId
            _sorterPoolMembers = membersMap
            _rawCeLength = rawCeLength
            _mutationMod = mutationMod
        }


module SorterPool = 

    let getAverageScore (measure: sorterEvalMeasure) (pool: sorterPool) : float<sorterEvalScore> =
            let scoreFunc = SorterEvalFunctions.getFunctionForMeasure measure
            let validScores =
                pool.SorterPoolMembers
                |> Seq.choose (fun spm -> spm.SorterEval |> Option.map scoreFunc)
                |> Seq.map UMX.untag
                |> Seq.toArray

            if Array.isEmpty validScores then
                Double.PositiveInfinity |> UMX.tag<sorterEvalScore>
            else
                Array.average validScores |> UMX.tag<sorterEvalScore>

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


    /// Updates the mutationMod for the pool and applies the change to all members (resetting their mutationIndex)
    let changeMutationMod (newMod: int<mutationMod>) (pool: sorterPool) : sorterPool =
        let updatedMembers =
            pool._sorterPoolMembers
            |> Map.map (fun _ memberObj ->
                memberObj |> SorterPoolMember.changeMutationMod newMod
            )

        { pool with 
            _mutationMod = newMod
            _sorterPoolMembers = updatedMembers }



    /// Applies the same mutantsPerSorter count to every pool member, accumulating 
    /// the advanced parents and all newly spawned mutants into a single updated pool.
    let mutate 
            (sorterModelMut: sorterModelMutator) 
            (mutantsPerSorter: int<sorterChildCount>)  
            (currentGeneration: int<generationNumber>)
            (pool: sorterPool) : sorterPool =

        let updatedMembersMap =
            pool.SorterPoolMembers
            |> Seq.fold (fun accMap currentMember ->
                // Invoke the member-level mutation strategy designed earlier
                let updatedParent, childMutants = 
                    SorterPoolMember.mutate 
                            sorterModelMut 
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


    /// Adjusts the RawCeLength to the minimal LastCeIndex required to keep at least 
    /// sortedFractionThreshold fraction of members sorted, and prunes any members exceeding that cutoff.
    let adjustCeLengthByThreshold
            (sortedFractionThreshold: float<sortedFraction>)
            (pool: sorterPool) : sorterPool =

        // 1. Gather all sorted members that have valid evaluations
        let sortedMembersWithLastIndex =
            pool.SorterPoolMembers
            |> Seq.choose (fun spm ->
                match spm.SorterEval with
                | Some eval when SorterEval.getIsSorted eval ->
                    Some (spm, SorterEval.getLastCeIndex eval)
                | _ -> None
            )
            |> Seq.toArray

        if Array.isEmpty sortedMembersWithLastIndex then
            // If no sorted members exist, keep pool unchanged
            pool
        else
            // 2. Sort by LastCeIndex ascending to determine the threshold index cutoff
            let sortedByLastCe = 
                sortedMembersWithLastIndex 
                |> Array.sortBy (fun (_, lastIdx) -> %lastIdx)

            // Calculate target count based on the threshold
            let targetCount = 
                sortedByLastCe.Length 
                |> float 
                |> (*) %sortedFractionThreshold 
                |> Math.Ceiling 
                |> int 
                |> max 1

            let targetIndex = min (targetCount - 1) (sortedByLastCe.Length - 1)
            let _, thresholdLastCeIndex = sortedByLastCe.[targetIndex]

            // 3. Filter out all pool members whose LastCeIndex exceeds the cutoff
            let updatedMembers =
                pool.SorterPoolMembers
                |> Seq.filter (fun spm ->
                    match spm.SorterEval with
                    | Some eval -> SorterEval.getLastCeIndex eval <= thresholdLastCeIndex
                    | None -> true
                )
                |> Seq.toArray

            // Re-create the pool with the newly calculated cutoff as RawCeLength
            sorterPool.create 
                pool.SorterPoolId 
                pool.Name 
                updatedMembers 
                (UMX.tag<ceLength> %thresholdLastCeIndex)
                pool.MutationMod



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
                if %filterUnsorted then
                    match spm.SorterEval with
                    | Some eval -> eval |> SorterEval.getIsShortEnough pool.RawCeLength 
                    | None -> false // Unevaluated members cannot verify if they are fully sorted
                else true
            )

        // if the hashes are the same, then prioritize the older member
        let birthdaySort =
                filter1 |> Seq.sortBy(fun spm -> spm.Birthday)

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
                    | None -> Double.PositiveInfinity |> UMX.tag<sorterEvalScore>
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

        sorterPool.create 
                    pool.SorterPoolId 
                    pool.Name 
                    sortedSurvivors
                    pool.RawCeLength 
                    pool.MutationMod



    /// Debug version of pruneSorterPool that forces immediate evaluation at each step
    /// to allow complete inspection of intermediate collections and count drop-offs.
    let pruneSorterPoolDebug
            (pool: sorterPool) 
            (measure: sorterEvalMeasure) 
            (prioritizeNewMutants: bool<prioritizeNewMutants>)
            (distinctSorterHashes: bool<distinctSorterHashes>)
            (sorterCountPerPool: int<sorterCountPerPool>) : sorterPool =

        let targetSize = max 0 %sorterCountPerPool
        let scoreFunc = SorterEvalFunctions.getFunctionForMeasure measure
        let filterUnsorted = SorterEvalFunctions.getFilterUnsortedFlag measure

        let initialMembers = pool.SorterPoolMembers |> Seq.toArray
        let initialCount = initialMembers.Length

        // --- Step 1: Filter Unsorted ---
        let filter1Members =
            initialMembers
            |> Array.filter (fun spm ->
                if %filterUnsorted then
                    match spm.SorterEval with
                    | Some eval -> eval |> SorterEval.getIsShortEnough pool.RawCeLength 
                    | None -> false // Unevaluated members cannot verify if they are fully sorted
                else true
            )

        let countAfterFilter1 = filter1Members.Length

        if countAfterFilter1 = 0 && initialCount > 0 && Debugger.IsAttached then
            Debugger.Break() // Pause if filtering unsorted wiped out all members

        // --- Step 2: Birthday Sort (Stable base order for deduplication) ---
        let birthdaySortedMembers =
            filter1Members 
            |> Array.sortBy (fun spm -> spm.Birthday)

        // --- Step 3: Distinct Hashes Deduplication ---
        let filter2Members =
            if %distinctSorterHashes then
                birthdaySortedMembers 
                |> Array.distinctBy (fun spm -> 
                    match spm.SorterEval with
                    | Some eval -> %(SorterEval.getSequenceHash eval)
                    | None -> 
                        if Debugger.IsAttached then Debugger.Break() // Unevaluated member reaching distinctBy step
                        0
                )
            else
                birthdaySortedMembers
        let countAfterFilter2 = filter2Members.Length

        if countAfterFilter2 = 0 && countAfterFilter1 > 0 && Debugger.IsAttached then
            Debugger.Break() // Pause if distinct hashing wiped out all members

        // --- Step 4: Scoring & Tuple Generation ---
        let scoredMembers =
            filter2Members
            |> Array.map (fun spm ->
                let score = 
                    match spm.SorterEval with
                    | Some eval -> scoreFunc eval
                    | None -> Double.PositiveInfinity |> UMX.tag<sorterEvalScore>
            
                let mIndexRaw = %spm.MutationIndex
                let tieBreaker = if %prioritizeNewMutants then mIndexRaw else -mIndexRaw
            
                (score, tieBreaker, spm)
            )

        // --- Step 5: Ranking & Sorting ---
        let rankedMembers =
            scoredMembers
            |> Array.sortBy (fun (score, tieBreaker, _) -> (score, tieBreaker))

        // --- Step 6: Truncation (Pruning to Target Capacity) ---
        let truncatedSurvivors =
            rankedMembers
            |> Array.truncate targetSize
            |> Array.map (fun (_, _, spm) -> spm)

        let finalCount = truncatedSurvivors.Length

        if finalCount = 0 && targetSize > 0 && countAfterFilter2 > 0 && Debugger.IsAttached then
            Debugger.Break() // Pause if final truncation resulted in an empty pool

        sorterPool.create 
                    pool.SorterPoolId 
                    pool.Name 
                    truncatedSurvivors
                    pool.RawCeLength 
                    pool.MutationMod