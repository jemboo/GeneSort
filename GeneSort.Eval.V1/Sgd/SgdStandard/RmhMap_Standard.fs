namespace GeneSort.Eval.V1.Sgd.Standard

open FSharp.UMX
open GeneSort.Eval.V1

type rmhMap_Standard = 
    private { 
        map: Map<Guid<sorterPoolId>, Map<Guid<sorterPoolMemberId>, spMemberHistory_Standard>> 
    }

    member this.SorterPoolMap = this.map

module RunningMemberHistoryMap =

    let empty : rmhMap_Standard = 
        { map = Map.empty }

    let create (map: Map<Guid<sorterPoolId>, Map<Guid<sorterPoolMemberId>, spMemberHistory_Standard>>) : rmhMap_Standard =
        { map = map }

    let toMap (runningMap: rmhMap_Standard) = runningMap.SorterPoolMap

    /// Incorporates newly generated members from an updated pool set into history tracking
    let updateFromPoolSet 
            (currentGen: int<generationNumber>)
            (poolSet: spSet_Standard) 
            (runningMap: rmhMap_Standard) : rmhMap_Standard =
    
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
                                    None
                                    currentGen 
                                    spm

                            Map.add spm.SorterPoolMemberId pmHist pmAcc
                        ) poolMap

                    Map.add poolId updatedPoolMap acc
            ) runningMap.SorterPoolMap

        { map = newMap }