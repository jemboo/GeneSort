namespace GeneSort.Eval.V1.Sgd.Standard

open FSharp.UMX
open GeneSort.Eval.V1

type mhMap_Standard = 
    private { 
        map: Map<Guid<sorterPoolId>, Map<Guid<sorterPoolMemberId>, spMemberHistory_Standard>> 
    }

    member this.SorterPoolMap = this.map

module MhMap_Standard =

    let empty : mhMap_Standard = 
        { map = Map.empty }

    let create (map: Map<Guid<sorterPoolId>, Map<Guid<sorterPoolMemberId>, spMemberHistory_Standard>>) : mhMap_Standard =
        { map = map }

    let toMap (runningMap: mhMap_Standard) = runningMap.SorterPoolMap

    /// Incorporates newly generated members from an updated pool set into history tracking
    let updateFromPoolSet 
            (currentGen: int<generationNumber>)
            (poolSet: sorterPoolSet_Standard) 
            (runningMap: mhMap_Standard) : mhMap_Standard =
    
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
                                SpMemberHistory_Standard.fromPoolMember 
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