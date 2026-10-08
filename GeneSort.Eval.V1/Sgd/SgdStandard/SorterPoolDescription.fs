namespace GeneSort.Eval.V1.Sgd.Standard
open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Model.Sorting.V1
open GeneSort.Core
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Eval.V1.Sgd

type spmDescription =
    private {
        _sorterPoolMemberId:   Guid<sorterPoolMemberId>
        _sorterModelId:        Guid<sorterModelId>
        _mutationIndex:        int<mutationIndex>
        _sorterMutationSource: sorterMutationSource option
        _sorterEvalMap:        Map<string<sortableTestsSubsetId>, sorterEval>
        _birthday:             int<generationNumber>
    }
    member this.Birthday with get() = this._birthday
    member this.SorterPoolMemberId with get() = this._sorterPoolMemberId
    member this.SorterModelId with get() = this._sorterModelId
    member this.MutationIndex with get() = this._mutationIndex
    member this.SorterMutationSource with get() = this._sorterMutationSource
    member this.SorterEvalMap with get() = this._sorterEvalMap

    static member create poolMemberId modelId mutationIndex mutationSource sorterEvalMap birthday =
        {
            _sorterPoolMemberId = poolMemberId
            _sorterModelId = modelId
            _mutationIndex = mutationIndex
            _sorterMutationSource = mutationSource
            _sorterEvalMap = sorterEvalMap
            _birthday = birthday
        }


module SpmDescription = 

    let toDataTableRecordsWithPrefix (prefix: string)
                                     (spmDesc: spmDescription) : dataTableRecord array =
        // 1. Map root scalar fields belonging directly to spmDescription
        let baseRecord =
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData (sprintf "%sSorterPoolMemberId" prefix) (string (%spmDesc.SorterPoolMemberId))
            |> dataTableRecord.addData (sprintf "%sSorterModelId" prefix) (string (%spmDesc.SorterModelId))
            |> dataTableRecord.addData (sprintf "%sMutationIndex" prefix) (string (%spmDesc.MutationIndex))
            |> dataTableRecord.addData (sprintf "%sBirthday" prefix) (string (%spmDesc.Birthday))

        // 2. Flatten optional sorterMutationSource properties if present
        let sourceRecord =
            match spmDesc.SorterMutationSource with
            | Some source -> source |> SorterMutationSource.toDataTableRecordWithPrefix prefix
            | None -> dataTableRecord.createEmpty()

        // 3. Emit one row per sortable-test subset evaluation.
        spmDesc.SorterEvalMap
        |> Map.toSeq
        |> Seq.map (fun (sortableTestsSubsetId, sorterEval) ->
            let subsetRecord =
                dataTableRecord.createEmpty()
                |> dataTableRecord.addData
                    (sprintf "%sSortableTestsSubsetId" prefix)
                    (string %sortableTestsSubsetId)

            sorterEval
            |> SorterEval.toDataTableRecordWithPrefix prefix
            |> dataTableRecord.combine subsetRecord
            |> dataTableRecord.combine sourceRecord
            |> dataTableRecord.combine baseRecord)
        |> Seq.toArray


type sorterPoolDescription =
    private {
        _sorterPoolId: Guid<sorterPoolId>
        _sorterPoolName: string<sorterPoolName>
        _sorterPoolMembers: spmDescription array
        _rawCeLength: int<ceLength>
    }

    member this.RawCeLength with get() = this._rawCeLength
    member this.SorterPoolId with get() = this._sorterPoolId
    member this.SorterPoolName with get() = this._sorterPoolName
    member this.SorterPoolMembers with get() = this._sorterPoolMembers

    static member create 
                    (poolId: Guid<sorterPoolId>) 
                    (sorterPoolName: string<sorterPoolName>) 
                    (rawCeLength: int<ceLength>) 
                    (spmDescriptions: spmDescription []) =
        { _sorterPoolId = poolId; 
          _rawCeLength = rawCeLength; 
          _sorterPoolName = sorterPoolName; 
          _sorterPoolMembers = spmDescriptions }


type sorterPoolSetDescription =
    private {
        _sorterPoolSetId: Guid<sorterPoolSetId>
        _generationNumber: int<generationNumber>
        _pools: sorterPoolDescription array
    }
    member this.SorterPoolSetId with get() = this._sorterPoolSetId
    member this.GenerationNumber with get() = this._generationNumber
    member this.Pools with get() = this._pools

    static member Create(setId, genNum, pools) =
        { _sorterPoolSetId = setId; _generationNumber = genNum; _pools = pools }


module SorterPoolSetDescription =

    /// Strips the heavy sorterModel references out of a pool set, creating a light memory footprint snapshot
    let fromPoolSet (poolSet: spSet_Standard) : sorterPoolSetDescription =
        let poolDescriptions =
            poolSet.SorterPools
            |> Map.values
            |> Seq.map (fun pool ->
                let memberDescriptions =
                    pool.SorterPoolMembers
                    |> Seq.map (fun spm ->
                        let modelId = SorterModel.getId spm.SorterModel
                        spmDescription.create
                            spm.SorterPoolMemberId
                            modelId
                            spm.MutationIndex
                            spm.SorterMutationSource
                            spm.SorterEvalMap
                            spm.Birthday
                    )
                    |> Seq.toArray

                sorterPoolDescription.create 
                            pool.SorterPoolId 
                            pool.Name 
                            pool.RawCeLength 
                            memberDescriptions
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
                |> dataTableRecord.addData (sprintf "%sRawCeLength" prefix) (string (%poolDesc.RawCeLength))
                |> dataTableRecord.addData (sprintf "%sSorterPoolName" prefix) (string (%poolDesc.SorterPoolName))

            // 2b. Emit one record for every sortable-test subset evaluation on each member.
            poolDesc.SorterPoolMembers
            |> Array.collect (fun memberDesc ->
                memberDesc 
                |> SpmDescription.toDataTableRecordsWithPrefix prefix
                |> Array.map (dataTableRecord.combine poolContextDtr)
            )
        )


    /// Extracts one dataTableRecord for every pool-member evaluation.  Each evaluation
    /// supplies SortableTestsSubsetId, which partitions a pool's snapshot statistics.
    let toDataTableRecordsSnapshot (prefix: string) (srRes: spSet_Standard) : dataTableRecord seq =
        let yab = fromPoolSet srRes
        toDataTableRecords prefix yab
