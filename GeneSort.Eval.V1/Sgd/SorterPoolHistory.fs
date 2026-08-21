namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Core
open GeneSort.Eval.V1

type sorterPoolHistory = 
    private {
        sorterPoolId: Guid<sorterPoolId>
        saveGeneration: int<generationNumber>
        memberHistories: sorterPoolMemberHistory list
    }

    static member create
            (sorterPoolId: Guid<sorterPoolId>,
             saveGeneration: int<generationNumber>,
             memberHistories: sorterPoolMemberHistory list) : sorterPoolHistory =
        {
            sorterPoolId = sorterPoolId
            saveGeneration = saveGeneration
            memberHistories = memberHistories
        }

    member this.SorterPoolId with get() = this.sorterPoolId
    member this.SaveGeneration with get() = this.saveGeneration
    member this.MemberHistories with get() = this.memberHistories


module SorterPoolHistory =

    /// Updates running tracked history for a pool with all members generated in currentGen,
    /// and then prunes all entries that do not belong to the ancestral tree of alive members.
    let pruneAndCreateForPool
            (currentGen: int<generationNumber>) 
            (pool: sorterPool)
            (runningPoolMemberHistory: Map<Guid<sorterPoolMemberId>, sorterPoolMemberHistory>) 
            : sorterPoolHistory * Map<Guid<sorterPoolMemberId>, sorterPoolMemberHistory> =

        // 1. Ingest all current members into tracking map
        let updatedTrackedMap = 
            pool.SorterPoolMembers 
            |> Seq.fold (fun acc spm ->
                if Map.containsKey spm.SorterPoolMemberId acc then acc
                else
                    let hist = SorterPoolMemberHistory.fromPoolMember pool.SorterPoolId currentGen spm
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
            sorterPoolHistory.create(
                sorterPoolId = pool.SorterPoolId,
                saveGeneration = currentGen,
                memberHistories = memberHistories
            )

        poolHistory, prunedTrackedMap

    let toDataTableRecords (history: sorterPoolHistory) : dataTableRecord list =
        history.MemberHistories 
        |> List.map SorterPoolMemberHistory.toDataTableRecord