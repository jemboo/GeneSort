namespace GeneSort.Eval.V1

open System
open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1

type spmDescription =
    private {
        _sorterPoolMemberId:   Guid<sorterPoolMemberId>
        _sorterModelId:        Guid<sorterModelId>
        _sorterMutationIndex:  int<sorterMutationIndex>
        _sorterMutationSource: sorterMutationSource option
        _sorterEval:           sorterEval option
    }
    member this.SorterPoolMemberId = this._sorterPoolMemberId
    member this.SorterModelId = this._sorterModelId
    member this.SorterMutationIndex = this._sorterMutationIndex
    member this.SorterMutationSource = this._sorterMutationSource
    member this.SorterEval = this._sorterEval

    static member Create poolMemberId modelId mutationIndex mutationSource evaluation =
        {
            _sorterPoolMemberId = poolMemberId
            _sorterModelId = modelId
            _sorterMutationIndex = mutationIndex
            _sorterMutationSource = mutationSource
            _sorterEval = evaluation
        }

type sorterPoolDescription =
    private {
        _sorterPoolId: Guid<sorterPoolId>
        _sorterPoolMembers: spmDescription array
    }
    member this.SorterPoolId = this._sorterPoolId
    member this.SorterPoolMembers = this._sorterPoolMembers

    static member Create(poolId, members) =
        { _sorterPoolId = poolId; _sorterPoolMembers = members }

type sorterPoolSetDescription =
    private {
        _sorterPoolSetId: Guid<sorterPoolSetId>
        _generationNumber: int<generationNumber>
        _pools: sorterPoolDescription array
    }
    member this.SorterPoolSetId = this._sorterPoolSetId
    member this.GenerationNumber = this._generationNumber
    member this.Pools = this._pools

    static member Create(setId, genNum, pools) =
        { _sorterPoolSetId = setId; _generationNumber = genNum; _pools = pools }


module SorterPoolSetDescription =

    /// Strips the heavy sorterModel references out of a pool set, creating a light memory footprint snapshot
    let fromPoolSet (poolSet: sorterPoolSet) : sorterPoolSetDescription =
        let poolDescriptions =
            poolSet.SorterPools
            |> Map.values
            |> Seq.map (fun pool ->
                let memberDescriptions =
                    pool.SorterPoolMembers
                    |> Seq.map (fun spm ->
                        let modelId = SorterModel.getId spm.SorterModel
                        spmDescription.Create 
                            spm.SorterPoolMemberId 
                            modelId 
                            spm.SorterMutationIndex 
                            spm.SorterMutationSource 
                            spm.SorterEval
                    )
                    |> Seq.toArray
                sorterPoolDescription.Create(pool.SorterPoolId, memberDescriptions)
            )
            |> Seq.toArray

        sorterPoolSetDescription.Create(poolSet.SorterPoolSetId, poolSet.GenerationNumber, poolDescriptions)