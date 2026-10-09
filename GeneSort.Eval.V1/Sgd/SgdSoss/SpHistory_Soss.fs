namespace GeneSort.Eval.V1.Sgd.Soss

open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1

type spHistory_Soss = 
    private {
        sorterPoolId: Guid<sorterPoolId>
        saveGeneration: int<generationNumber>
        memberHistories: spMemberHistory_Soss list
    }

    static member create
            (sorterPoolId: Guid<sorterPoolId>,
             saveGeneration: int<generationNumber>,
             memberHistories: spMemberHistory_Soss list) : spHistory_Soss =
        {
            sorterPoolId = sorterPoolId
            saveGeneration = saveGeneration
            memberHistories = memberHistories
        }

    member this.SorterPoolId with get() = this.sorterPoolId
    member this.SaveGeneration with get() = this.saveGeneration
    member this.MemberHistories with get() = this.memberHistories


module SpHistory_Soss =

    /// Updates running tracked history for a pool with all members generated in currentGen,
    /// and then prunes all entries that do not belong to the ancestral tree of alive members.
    let pruneAndCreateForPool
            (currentGen: int<generationNumber>) 
            (pool: sorterPool_Soss)
            (runningPoolMemberHistory: Map<Guid<sorterPoolMemberId>, spMemberHistory_Soss>) 
            : spHistory_Soss * Map<Guid<sorterPoolMemberId>, spMemberHistory_Soss> =

        // 1. Ingest all current members directly using ParentSorterPoolMemberId
        let updatedTrackedMap = 
            pool.SorterPoolMembers 
            |> Seq.fold (fun acc spm ->
                if Map.containsKey spm.SorterPoolMemberId acc then acc
                else
                    let parentMemberId =
                        spm.SorterMutationSource
                        |> Option.map (fun src -> src.SorterPoolMemberId)
                    let parentPoolId =
                        spm.SorterMutationSource
                        |> Option.map (fun src -> src.SorterPoolId)

                    let hist = SpMemberHistory_Soss.fromPoolMember 
                                    pool.SorterPoolId parentMemberId 
                                    parentPoolId currentGen spm
                    Map.add spm.SorterPoolMemberId hist acc
            ) runningPoolMemberHistory

        // 2. Identify alive member IDs
        let aliveMemberIds = 
            pool.SorterPoolMembers 
            |> Seq.map (fun spm -> spm.SorterPoolMemberId) 
            |> Set.ofSeq

        // 3. Trace back ancestors of all alive members
        let rec collectAncestors (toVisit: Guid<sorterPoolMemberId> list) (visited: Set<Guid<sorterPoolMemberId>>) =
            match toVisit with
            | [] -> visited
            | currentId :: rest ->
                if Set.contains currentId visited then
                    collectAncestors rest visited
                else
                    let newVisited = Set.add currentId visited
                    let parentIdOpt = 
                        Map.tryFind currentId updatedTrackedMap 
                        |> Option.bind (fun h -> h.ParentSorterPoolMemberId)

                    match parentIdOpt with
                    | Some parentId when Map.containsKey parentId updatedTrackedMap ->
                        collectAncestors (parentId :: rest) newVisited
                    | _ -> 
                        collectAncestors rest newVisited

        let keptMemberIds = collectAncestors (Set.toList aliveMemberIds) Set.empty

        // 4. Prune entries from history map that have no living descendants
        let prunedTrackedMap = 
            updatedTrackedMap 
            |> Map.filter (fun id _ -> Set.contains id keptMemberIds)

        let memberHistories = 
            prunedTrackedMap 
            |> Map.toList 
            |> List.map snd

        let poolHistory = 
            spHistory_Soss.create(
                sorterPoolId = pool.SorterPoolId,
                saveGeneration = currentGen,
                memberHistories = memberHistories
            )

        poolHistory, prunedTrackedMap

    let toDataTableRecords (history: spHistory_Soss) : dataTableRecord list =
        history.MemberHistories 
        |> List.map SpMemberHistory_Soss.toDataTableRecord