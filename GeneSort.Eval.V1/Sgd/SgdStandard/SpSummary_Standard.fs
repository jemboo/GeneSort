namespace GeneSort.Eval.V1.Sgd.Standard

open GeneSort.Eval.V1.Sgd

open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Core


type spSummary_Standard =
    private {
        _sorterPoolId: Guid<sorterPoolId>
        _sorterPoolName: string<sorterPoolName>
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


type sorterPoolSetSummary_Standard =
    private {
        _sorterPoolSetId: Guid<sorterPoolSetId>
        _generationNumber: int<generationNumber>
        _sortedSorterEvalPercentage: float
        _sorterPoolSummaries: spSummary_Standard array
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


module SorterPoolSetSummary_Standard =

    let getMaxGeneration (spses: sorterPoolSetSummary_Standard array) : int<generationNumber> =
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
    let fromPoolSet (poolSet: sorterPoolSet_Standard) : sorterPoolSetSummary_Standard =
        
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
                    spSummary_Standard.create
                        pool.SorterPoolId
                        pool.Name
                        pool.RawCeLength
                        (0 |> UMX.tag)
                        (0.0 |> UMX.tag)
                        (0.0 |> UMX.tag)
                        (0 |> UMX.tag)
                        (0.0 |> UMX.tag)
                        (0.0 |> UMX.tag)
                        (0.0 |> UMX.tag)
                        (0.0 |> UMX.tag)
                        0.0
                else
                    // Map out the metrics across all evaluations
                    let ceLengths = evals |> Array.map (fun ev -> float %(SorterEval.getCeLength ev))
                    let stageLengths = evals |> Array.map (fun ev -> float %(SorterEval.getStageLength ev))
                    let stageCrossings = evals |> Array.map (fun ev -> float %(SorterEval.getStageCrossingsCount ev))
                    let reflectiveCountRs = evals |> Array.map (fun ev -> float (float %(SorterEval.getReflectiveCount ev) / float %(SorterEval.getCeLength ev)))
                    let averageUnsortedCount = evals |> Array.averageBy (fun ev -> float (UMX.untag (SorterEval.getUnsortedCount ev)))

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

                    spSummary_Standard.create 
                        pool.SorterPoolId 
                        pool.Name 
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
            )
            |> Seq.toArray

        // 2. Wrap the final payload up into the collection summary
        sorterPoolSetSummary_Standard.Create(
            poolSet.SorterPoolSetId, 
            poolSet.GenerationNumber, 
            poolSet.SortedSorterEvalPercentage,
            poolSummaries
        )

    /// Flattens the hierarchical summary structure into an array of flat dataTableRecords 
    /// containing pool set context alongside individual pool summary metrics.
    let toDataTableRecords (prefix: string) (summarySet: sorterPoolSetSummary_Standard) : dataTableRecord array =
        
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
