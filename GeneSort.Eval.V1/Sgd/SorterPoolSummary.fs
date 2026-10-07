namespace GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Core


type sorterPoolSummary =
    private {
        _sorterPoolId: Guid<sorterPoolId>
        _sorterPoolName: string<sorterPoolName>
        _sortableTestsSubsetId: string<sortableTestsSubsetId>
        _minCeLength: int<ceLength>
        _aveCeLength: float<ceLength>
        _stdDevCeLength: float<ceLength>
        _minStageLength: int<stageLength>
        _aveStageLength: float<stageLength>
        _stdDevStageLength: float<stageLength>
        _rawCeLength: int<ceLength>
        _aveStageCrossings: float<stageCrossings>
        _aveReflectiveCountR: float<reflectiveCount>
        _averageUnsortedCount: float
    }

    member this.RawCeLength with get() = this._rawCeLength
    member this.SorterPoolId with get() = this._sorterPoolId
    member this.SorterPoolName with get() = this._sorterPoolName
    member this.SortableTestsSubsetId with get() = this._sortableTestsSubsetId
    member this.AveCeLength with get() = this._aveCeLength
    member this.StdDevCeLength with get() = this._stdDevCeLength
    member this.MinCeLength with get() = this._minCeLength
    member this.MinStageLength with get() = this._minStageLength
    member this.AveStageLength with get() = this._aveStageLength
    member this.StdDevStageLength with get() = this._stdDevStageLength
    member this.AveStageCrossings with get() = this._aveStageCrossings
    member this.AveReflectiveCountR with get() = this._aveReflectiveCountR
    member this.AverageUnsortedCount with get() = this._averageUnsortedCount


    static member create 
                    (poolId: Guid<sorterPoolId>) 
                    (sorterPoolName: string<sorterPoolName>) 
                    (sortableTestsSubsetId: string<sortableTestsSubsetId>)
                    (rawCeLength: int<ceLength>) 
                    (minCeLength: int<ceLength>) 
                    (aveCeLength: float<ceLength>) 
                    (stdDevCeLength: float<ceLength>) 
                    (minStageLength: int<stageLength>) 
                    (aveStageLength: float<stageLength>) 
                    (stdDevStageLength: float<stageLength>) 
                    (aveStageCrossings: float<stageCrossings>) 
                    (aveReflectiveCountR: float<reflectiveCount>)
                    (averageUnsortedCount: float) =
        { 
          _sorterPoolId = poolId; 
          _sorterPoolName = sorterPoolName;
          _sortableTestsSubsetId = sortableTestsSubsetId
          _rawCeLength = rawCeLength; 
          _minCeLength = minCeLength;
          _aveCeLength = aveCeLength; 
          _stdDevCeLength = stdDevCeLength; 
          _minStageLength = minStageLength; 
          _aveStageLength = aveStageLength;
          _stdDevStageLength = stdDevStageLength;
          _aveStageCrossings = aveStageCrossings;
          _aveReflectiveCountR = aveReflectiveCountR;
          _averageUnsortedCount = averageUnsortedCount;
        }


type sorterPoolSetSummary =
    private {
        _sorterPoolSetId: Guid<sorterPoolSetId>
        _generationNumber: int<generationNumber>
        _sortedSorterEvalPercentage: float
        _sorterPoolSummaries: sorterPoolSummary array
    }
    member this.SorterPoolSetId with get() = this._sorterPoolSetId
    member this.GenerationNumber with get() = this._generationNumber
    member this.SortedSorterEvalPercentage with get() = this._sortedSorterEvalPercentage
    member this.SorterPoolSummaries with get() = this._sorterPoolSummaries

    static member Create(setId, genNum, sortedSorterEvalPercentage, summaries) =
        { _sorterPoolSetId = setId
          _generationNumber = genNum
          _sortedSorterEvalPercentage = sortedSorterEvalPercentage
          _sorterPoolSummaries = summaries }


module SorterPoolSetSummary =

    let getMaxGeneration (spses: sorterPoolSetSummary array) : int<generationNumber> =
        let mv = spses |> Array.maxBy(fun spss -> %spss.GenerationNumber)
        mv.GenerationNumber

    /// Computes population standard deviation of an un-tagged float sequence
    let private computeStdDev (values: float array) (mean: float) : float =
        if values.Length <= 1 then 
            0.0 
        else
            let variance = values |> Array.averageBy (fun x -> (x - mean) ** 2.0)
            sqrt variance

    /// Strips the heavy sorterModel references out of a pool set, creating a light memory footprint snapshot
    let fromPoolSet (poolSet: sorterPoolSet) : sorterPoolSetSummary =
        
        // 1. Process each pool within the pool set
        let poolSummaries = 
            poolSet.SorterPools 
            |> Seq.collect (fun (KeyValue(_, pool)) ->
                let evalsBySubset =
                    pool.SorterPoolMembers
                    |> Seq.collect (fun memberObj -> memberObj.SorterEvalMap |> Map.toSeq)
                    |> Seq.groupBy fst

                evalsBySubset
                |> Seq.map (fun (sortableTestsSubsetId, subsetEvals) ->
                    let evals = subsetEvals |> Seq.map snd |> Seq.toArray

                    // Map out the metrics across all evaluations
                    let ceLengths = evals |> Array.map (fun ev -> float %(SorterEval.getCeLength ev))
                    let stageLengths = evals |> Array.map (fun ev -> float %(SorterEval.getStageLength ev))
                    let stageCrossings = evals |> Array.map (fun ev -> float %(SorterEval.getStageCrossingsCount ev))
                    let reflectiveCountRs = evals |> Array.map (fun ev -> float (float %(SorterEval.getReflectiveCount ev) / float %(SorterEval.getCeLength ev)))
                    let averageUnsortedCount =
                        evals
                        |> Array.choose (SorterEval.getUnsortedCount >> Option.map UMX.untag >> Option.map float)
                        |> function
                            | [||] -> 0.0
                            | counts -> Array.average counts

                    // Compute minimums
                    let minCe = (Array.min ceLengths |> int) |> UMX.tag<ceLength>
                    let minStage = (Array.min stageLengths |> int) |> UMX.tag<stageLength>

                    // Compute averages
                    let aveCeVal = ceLengths |> Array.average
                    let aveStageVal = stageLengths |> Array.average
                    let aveCe = aveCeVal |> UMX.tag<ceLength>
                    let aveStage = aveStageVal |> UMX.tag<stageLength>
                    let aveStageCrossings = (stageCrossings |> Array.average) |> UMX.tag<stageCrossings>
                    let aveReflectiveCountR = (reflectiveCountRs |> Array.average) |> UMX.tag<reflectiveCount>

                    // Compute standard deviations
                    let stdDevCe = computeStdDev ceLengths aveCeVal |> UMX.tag<ceLength>
                    let stdDevStage = computeStdDev stageLengths aveStageVal |> UMX.tag<stageLength>

                    sorterPoolSummary.create 
                        pool.SorterPoolId 
                        pool.Name 
                        sortableTestsSubsetId
                        pool.RawCeLength 
                        minCe 
                        aveCe
                        stdDevCe
                        minStage 
                        aveStage
                        stdDevStage
                        aveStageCrossings
                        aveReflectiveCountR
                        averageUnsortedCount
                ))
            |> Seq.toArray

        // 2. Wrap the final payload up into the collection summary
        sorterPoolSetSummary.Create(
            poolSet.SorterPoolSetId, 
            poolSet.GenerationNumber, 
            poolSet.SortedSorterEvalPercentage,
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
            |> dataTableRecord.addData (sprintf "%sSortedSorterEvalPercentage" prefix) (sprintf "%.2f%%" summarySet.SortedSorterEvalPercentage)

        // 2. Iterate through each pool summary and combine metrics with the root context
        summarySet.SorterPoolSummaries
        |> Array.map (fun poolSum ->
            setContextDtr
            |> dataTableRecord.addData (sprintf "%sSorterPoolId" prefix) (string (%poolSum.SorterPoolId))
            |> dataTableRecord.addData (sprintf "%sSorterPoolName" prefix) (string (%poolSum.SorterPoolName))
            |> dataTableRecord.addData (sprintf "%sSortableTestsSubsetId" prefix) (string (%poolSum.SortableTestsSubsetId))
            |> dataTableRecord.addData (sprintf "%sRawCeLength" prefix) (string (%poolSum.RawCeLength))
            |> dataTableRecord.addData (sprintf "%sMinCeLength" prefix) (string (%poolSum.MinCeLength))
            |> dataTableRecord.addData (sprintf "%sAveCeLength" prefix) (sprintf "%.5f" (%poolSum.AveCeLength))
            |> dataTableRecord.addData (sprintf "%sStdDevCeLength" prefix) (sprintf "%.5f" (%poolSum.StdDevCeLength))
            |> dataTableRecord.addData (sprintf "%sMinStageLength" prefix) (string (%poolSum.MinStageLength))
            |> dataTableRecord.addData (sprintf "%sAveStageLength" prefix) (sprintf "%.5f" (%poolSum.AveStageLength))
            |> dataTableRecord.addData (sprintf "%sStdDevStageLength" prefix) (sprintf "%.5f" (%poolSum.StdDevStageLength))
            |> dataTableRecord.addData (sprintf "%sAveStageCrossings" prefix) (sprintf "%.5f" (%poolSum.AveStageCrossings))
            |> dataTableRecord.addData (sprintf "%sAveReflectiveCountR" prefix) (sprintf "%.5f" (%poolSum.AveReflectiveCountR))
            |> dataTableRecord.addData (sprintf "%sAverageUnsortedCount" prefix) (sprintf "%.2f" poolSum.AverageUnsortedCount)
        )
