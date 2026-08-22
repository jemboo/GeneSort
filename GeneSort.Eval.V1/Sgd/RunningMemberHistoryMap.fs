namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.Eval.V1

type runningMemberHistoryMap = 
    private { 
        map: Map<Guid<sorterPoolId>, Map<Guid<sorterPoolMemberId>, sorterPoolMemberHistory>> 
    }

    member this.SorterPoolMap = this.map

module RunningMemberHistoryMap =

    let empty : runningMemberHistoryMap = 
        { map = Map.empty }

    let create (map: Map<Guid<sorterPoolId>, Map<Guid<sorterPoolMemberId>, sorterPoolMemberHistory>>) : runningMemberHistoryMap =
        { map = map }

    let toMap (runningMap: runningMemberHistoryMap) = runningMap.SorterPoolMap

    /// Incorporates newly generated members from an updated pool set into history tracking
    let updateFromPoolSet 
            (currentGen: int<generationNumber>)
            (poolSet: sorterPoolSet) 
            (runningMap: runningMemberHistoryMap) : runningMemberHistoryMap =
    
        let newMap = 
            poolSet.SorterPools
            |> Map.fold (fun acc poolId pool ->
                let poolMap = Map.tryFind poolId acc |> Option.defaultValue Map.empty
            
                // Collect only un-tracked members to minimize intermediate Map reconstructions
                let newMembers = 
                    pool.SorterPoolMembers 
                    |> Seq.filter (fun spm -> not (Map.containsKey spm.SorterPoolMemberId poolMap))

                if Seq.isEmpty newMembers then
                    acc
                else
                    let updatedPoolMap = 
                        newMembers 
                        |> Seq.fold (fun pmAcc spm ->
                            let parentMemberId = 
                                spm.SorterMutationSource 
                                |> Option.map (fun src -> src.SorterPoolMemberId)

                            let pmHist = 
                                SorterPoolMemberHistory.fromPoolMember 
                                    poolId 
                                    parentMemberId 
                                    currentGen 
                                    spm

                            Map.add spm.SorterPoolMemberId pmHist pmAcc
                        ) poolMap

                    Map.add poolId updatedPoolMap acc
            ) runningMap.SorterPoolMap

        { map = newMap }