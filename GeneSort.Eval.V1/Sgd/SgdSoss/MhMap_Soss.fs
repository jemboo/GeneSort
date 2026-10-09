namespace GeneSort.Eval.V1.Sgd.Soss

open FSharp.UMX
open GeneSort.Eval.V1

type mhMap_Soss = 
    private { 
        map: Map<Guid<sorterPoolId>, Map<Guid<sorterPoolMemberId>, spMemberHistory_Soss>> 
    }

    member this.SorterPoolMap = this.map

module MhMap_Soss =

    let empty : mhMap_Soss = 
        { map = Map.empty }

    let create (map: Map<Guid<sorterPoolId>, Map<Guid<sorterPoolMemberId>, spMemberHistory_Soss>>) : mhMap_Soss =
        { map = map }

    let toMap (runningMap: mhMap_Soss) = runningMap.SorterPoolMap

    /// Incorporates newly generated members from an updated pool set into history tracking
    let updateFromPoolSet 
            (currentGen: int<generationNumber>)
            (poolSet: sorterPoolSet_Soss) 
            (runningMap: mhMap_Soss) : mhMap_Soss =
    
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
                                SpMemberHistory_Soss.fromPoolMember 
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