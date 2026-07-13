namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Core


type sorterPoolSummary =
    private {
        _sorterPoolId: Guid<sorterPoolId>
        _sorterPoolName: string<sorterPoolName>
        _aveCeLength: int<ceLength>
        _minCeLength: int<ceLength>
        _minStageLength: int<stageLength>
        _aveStageLength: int<stageLength>
        _rawCeLength: int<ceLength>
    }

    member this.RawCeLength with get() = this._rawCeLength
    member this.SorterPoolId with get() = this._sorterPoolId
    member this.SorterPoolName with get() = this._sorterPoolName
    member this.AveCeLength with get() = this._aveCeLength
    member this.MinCeLength with get() = this._minCeLength
    member this.MinStageLength with get() = this._minStageLength
    member this.AveStageLength with get() = this._aveStageLength

    static member create 
                    (poolId: Guid<sorterPoolId>) 
                    (sorterPoolName: string<sorterPoolName>) 
                    (rawCeLength: int<ceLength>) 
                    (minCeLength: int<ceLength>) 
                    (aveCeLength: int<ceLength>) 
                    (minStageLength: int<stageLength>) 
                    (aveStageLength: int<stageLength>) =
        { _sorterPoolId = poolId; 
          _sorterPoolName = sorterPoolName;
          _rawCeLength = rawCeLength; 
          _aveCeLength = aveCeLength; 
          _minCeLength = minCeLength 
          _minStageLength = minStageLength; 
          _aveStageLength = aveStageLength 
          }


type sorterPoolSetSummary =
    private {
        _sorterPoolSetId: Guid<sorterPoolSetId>
        _generationNumber: int<generationNumber>
        _sorterPoolSummaries: sorterPoolSummary array
    }
    member this.SorterPoolSetId with get() = this._sorterPoolSetId
    member this.GenerationNumber with get() = this._generationNumber
    member this.SorterPoolSummaries with get() = this._sorterPoolSummaries

    static member Create(setId, genNum, summaries) =
        { _sorterPoolSetId = setId; _generationNumber = genNum; _sorterPoolSummaries = summaries }


module SorterPoolSetSummary =

    /// Strips the heavy sorterModel references out of a pool set, creating a light memory footprint snapshot
    let fromPoolSet (poolSet: sorterPoolSet) : sorterPoolSetSummary =
        
        // 1. Process each pool within the pool set
        let poolSummaries = 
            poolSet.SorterPools 
            |> Seq.map (fun (KeyValue(_, pool)) ->
                
                // Get evaluations for all evaluated members in this pool
                let evals = 
                    pool.SorterPoolMembers
                    |> Seq.choose (fun memberObj -> memberObj.SorterEval)
                    |> Seq.toArray

                // Defensive check if a pool contains no evaluated members yet
                if Array.isEmpty evals then
                    sorterPoolSummary.create 
                        pool.SorterPoolId 
                        pool.Name 
                        pool.RawCeLength 
                        (0 |> UMX.tag) 
                        (0 |> UMX.tag) 
                        (0 |> UMX.tag) 
                        (0 |> UMX.tag)
                else
                    // Map out the metrics across all evaluations
                    let ceLengths = evals |> Array.map (fun ev -> % (SorterEval.getCeLength ev))
                    let stageLengths = evals |> Array.map (fun ev -> % (SorterEval.getStageLength ev))

                    // Compute minimums
                    let minCe = Array.min ceLengths |> UMX.tag<ceLength>
                    let minStage = Array.min stageLengths |> UMX.tag<stageLength>

                    // Compute averages safely as integers
                    let aveCe = (ceLengths |> Array.averageBy float |> int) |> UMX.tag<ceLength>
                    let aveStage = (stageLengths |> Array.averageBy float |> int) |> UMX.tag<stageLength>

                    sorterPoolSummary.create 
                        pool.SorterPoolId 
                        pool.Name 
                        pool.RawCeLength 
                        minCe 
                        aveCe 
                        minStage 
                        aveStage
            )
            |> Seq.toArray

        // 2. Wrap the final payload up into the collection summary
        sorterPoolSetSummary.Create(
            poolSet.SorterPoolSetId, 
            poolSet.GenerationNumber, 
            poolSummaries
        )

    /// Flattens the hierarchical summary structure into an array of flat dataTableRecords 
    /// containing pool set context alongside individual pool summary metrics.
    let toDataTableRecords (prefix: string) (summarySet: sorterPoolSetSummary) : dataTableRecord array =
        
        // 1. Establish the highest-level context columns
        let setContextDtr =
            dataTableRecord.createEmpty()
            |> dataTableRecord.addData (sprintf "%sSorterPoolSetId" prefix) (string (%summarySet.SorterPoolSetId))
            |> dataTableRecord.addData (sprintf "%sGenerationNumber" prefix) (string (%summarySet.GenerationNumber))

        // 2. Iterate through each pool summary and combine metrics with the root context
        summarySet.SorterPoolSummaries
        |> Array.map (fun poolSum ->
            setContextDtr
            |> dataTableRecord.addData (sprintf "%sSorterPoolId" prefix) (string (%poolSum.SorterPoolId))
            |> dataTableRecord.addData (sprintf "%sSorterPoolName" prefix) (string (%poolSum.SorterPoolName))
            |> dataTableRecord.addData (sprintf "%sRawCeLength" prefix) (string (%poolSum.RawCeLength))
            |> dataTableRecord.addData (sprintf "%sAveCeLength" prefix) (string (%poolSum.AveCeLength))
            |> dataTableRecord.addData (sprintf "%sMinCeLength" prefix) (string (%poolSum.MinCeLength))
            |> dataTableRecord.addData (sprintf "%sMinStageLength" prefix) (string (%poolSum.MinStageLength))
            |> dataTableRecord.addData (sprintf "%sAveStageLength" prefix) (string (%poolSum.AveStageLength))
        )