namespace GeneSort.Eval.V1

open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Core

type spmDescription =
    private {
        _sorterPoolMemberId:   Guid<sorterPoolMemberId>
        _sorterModelId:        Guid<sorterModelId>
        _mutationIndex:        int<mutationIndex>
        _sorterMutationSource: sorterMutationSource option
        _sorterEval:           sorterEval option
    }
    member this.SorterPoolMemberId = this._sorterPoolMemberId
    member this.SorterModelId = this._sorterModelId
    member this.MutationIndex = this._mutationIndex
    member this.SorterMutationSource = this._sorterMutationSource
    member this.SorterEval = this._sorterEval

    static member Create poolMemberId modelId mutationIndex mutationSource evaluation =
        {
            _sorterPoolMemberId = poolMemberId
            _sorterModelId = modelId
            _mutationIndex = mutationIndex
            _sorterMutationSource = mutationSource
            _sorterEval = evaluation
        }


module SpmDescription = 

    let toDataTableRecordWithPrefix (prefix: string) 
                                    (eval: spmDescription) : dataTableRecord =
        // 1. Map root scalar fields belonging directly to spmDescription
        let baseRecord =
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData (sprintf "%sSorterPoolMemberId" prefix) (string (%eval.SorterPoolMemberId))
            |> dataTableRecord.addData (sprintf "%sSorterModelId" prefix) (string (%eval.SorterModelId))
            |> dataTableRecord.addData (sprintf "%sMutationIndex" prefix) (string (%eval.MutationIndex))

        // 2. Flatten optional sorterMutationSource properties if present
        let sourceRecord =
            match eval.SorterMutationSource with
            | Some source -> source |> SorterMutationSource.toDataTableRecordWithPrefix prefix
            | None -> dataTableRecord.createEmpty()

        // 3. Flatten optional sorterEval metrics if present
        let evalRecord =
            match eval.SorterEval with
            | Some sorterEval -> sorterEval |> SorterEval.toDataTableRecordWithPrefix prefix
            | None -> dataTableRecord.createEmpty()

        // 4. Structural aggregation using the dataTableRecord combinators
        baseRecord
        |> dataTableRecord.combine sourceRecord
        |> dataTableRecord.combine evalRecord


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
                            spm.MutationIndex 
                            spm.SorterMutationSource 
                            spm.SorterEval
                    )
                    |> Seq.toArray
                sorterPoolDescription.Create(pool.SorterPoolId, memberDescriptions)
            )
            |> Seq.toArray

        sorterPoolSetDescription.Create(poolSet.SorterPoolSetId, poolSet.GenerationNumber, poolDescriptions)


    /// Flattens the hierarchical description structure into an array of flat dataTableRecords 
    /// containing pool set context, pool context, and individual pool member features.
    let toDataTableRecords (prefix: string) (setDesc: sorterPoolSetDescription) : dataTableRecord array =
        
        // 1. Establish the highest-level context columns
        let setContextDtr =
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData (sprintf "%sSorterPoolSetId" prefix) (string (%setDesc.SorterPoolSetId))
            |> dataTableRecord.addData (sprintf "%sGenerationNumber" prefix) (string (%setDesc.GenerationNumber))

        // 2. Drill down and collect records across pools and member arrays
        setDesc.Pools
        |> Array.collect (fun poolDesc ->
            
            // 2a. Layer down the pool level context
            let poolContextDtr =
                setContextDtr
                |> dataTableRecord.addData (sprintf "%sSorterPoolId" prefix) (string (%poolDesc.SorterPoolId))

            // 2b. Map every single pool member, flattening and combining structures upward
            poolDesc.SorterPoolMembers
            |> Array.map (fun memberDesc ->
                memberDesc 
                |> SpmDescription.toDataTableRecordWithPrefix prefix
                |> dataTableRecord.combine poolContextDtr
            )
        )