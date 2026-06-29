namespace GeneSort.Dispatch.V1.SorterSgd.Msrs

open System
open System.Threading
open FsToolkit.ErrorHandling
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.SortingOps
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Sorting.Sortable
open GeneSort.Dispatch.V1
open GeneSort.Model.Sortable.V1
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Dispatch.V1.SortableTest
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Eval.V1
open GeneSort.Eval.V1.Bins
open GeneSort.Dispatch.V1.CommonParams
open GeneSort.Dispatch.V1.SorterSgd


module MsrsSgdExecutor =

    let makeMutantSorterModels (rp:runParameters) : Async<Result<sorterModel seq, string>> =
        asyncResult {

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (sortingWidth: int<sortingWidth>) = 
                        rp.GetSortingWidth() 
                        |> Result.ofOption "Missing sorting width in run parameters"
    
            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (sorterChildCount: int<sorterChildCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorterChildCount in run parameters"

            let! (orthoRate: float<orthoRate>) =  
                        rp.GetOrthoRate()
                        |> Result.ofOption "Missing orthoRate in run parameters"

            let! (paraRate: float<paraRate>) =  
                        rp.GetParaRate()
                        |> Result.ofOption "Missing paraRate in run parameters"

            let! (selfSymRate: float<selfSymRate>) =  
                        rp.GetSelfSymRate()
                        |> Result.ofOption "Missing selfSymRate in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            let! (sest: sorterEvalSelectionType) = 
                        rp.GetSorterEvalSelectionType()
                        |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"

            let! (sem:sorterEvalMeasure) = 
                        rp.GetSorterEvalMeasure()
                        |> Result.ofOption "Missing sorterEvalMeasure in run parameters"

            let! (genCt: int<generationNumber>) =
                        rp.GetGenerationLast()
                        |> Result.ofOption "Missing generationCount in run parameters"

            let rngFactory = rngType |> RngFactory.create

            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getStandardSorterEvals 
                                            sortingWidth 
                                            simpleSorterModelType
                                            sorterEvalType.V2

            let _sorterEvalSelection = 
                            SorterEvalSelection.makeSelection 
                                        sem 
                                        sest
                                        parentSorterSetEval.SorterEvals
                                        parentSorterSetEval.SorterTestId

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        sortingWidth 
                                        simpleSorterModelType

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let sorterModelMutator = SimpleSorterModelMutator.getMsrsModelMutator
                                            rngFactory
                                            ExcludeSelfCe
                                            modificationRate
                                            orthoRate
                                            paraRate
                                            selfSymRate
                                     |> sorterModelMutator.Simple

            let childIndexes = [| 0 .. (%sorterChildCount - 1) |]

            // Streaming engine via sequence expression
            let generateMutantStream (parents: sorterModel[]) =
                seq {
                    for parentModel in parents do
                        for dex in childIndexes do
                            yield SorterModelMutator.makeMutantSorterModelFromIndex
                                        sorterModelMutator
                                        parentModel
                                        (dex |> UMX.tag<mutationIndex>)
                }

            return generateMutantStream parentSorterModelSet.SorterModels
        }



    /// Dispatches the evolution history run parameters, executes the generative loop via asyncResult,
    /// and manages final state serialization/reporting pipelines.
    let evaluateEvolutionRun
            (makeSortableTests: runParameters -> Async<Result<sortableTest, string>>)
            (sorterPoolSetCreator: runParameters -> Async<Result<sorterPoolSet, string>>)
            (host: IRunHost)
            (rp: runParameters)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (progress: IProgress<string> option) : Async<Result<runParameters, string>> =

        let log msg = 
            OpsUtils.report progress 
                (sprintf "%s [%s] %s" (MathUtils.getTimestampString()) (rp |> RunParameters.getIdString) msg)

        asyncResult {
            try
                do! checkCancellation cts.Token

                // 1. Gather all required run metrics and options out of your parameters block securely
                let! genLast = rp.GetGenerationLast() |> Result.ofOption "Missing genLast."
                let! genFirst = rp.GetGenerationFirst() |> Result.ofOption "Missing genFirst."
                let! sortersPerPool = rp.GetSorterCountPerPool() |> Result.ofOption "Missing sortersPerPool."
                let! sorterChildCount = rp.GetSorterChildCount() |> Result.ofOption "Missing sorter child count"
                let! sorterEvalMeasure = rp.GetSorterEvalMeasure() |> Result.ofOption "Missing sorterEvalMeasure."
                let! sorterEvalType = rp.GetSorterEvalType() |> Result.ofOption "Missing sorterEvalType."
                let! rngType = rp.GetRngType() |> Result.ofOption "Missing rngType."

                // 2. Resolve target seed sorterPoolSet collection state depending on genFirst criteria
                let! seedSorterPoolSet = 
                    if %genFirst > 0 then
                        log "Looking up historical sorterPoolSet from database..."
                        let newRp = rp.WithQueryWithGenFirst (Some true)
                        
                        let qpSRRResult = 
                            host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.SorterRunResult "")
                            |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

                        asyncResult {
                            let! qpSRR = qpSRRResult 
                            let! (outData: outputData) = 
                                host.RunDb.loadAsync qpSRR 
                                |> AsyncResult.mapError (fun err -> sprintf "Database load error: %A" err)

                            let! sorterRunRes = outData |> OutputData.asSorterRunResult
                            return sorterRunRes.FinalPoolSet
                        }
                    else
                        // Otherwise create it ..
                        log "Make seedSorterPoolSet..."
                        sorterPoolSetCreator rp

                do! checkCancellation cts.Token
                log "Executing makeSortableTests..."
                let! sortableTest = makeSortableTests rp
                
                log "Making sorterModelMutator..."
                // Rates for mutator creation

                let! (orthoRate: float<orthoRate>) = rp.GetOrthoRate() |> Result.ofOption "Missing orthoRate in run parameters"
                let! (paraRate: float<paraRate>) = rp.GetParaRate() |> Result.ofOption "Missing paraRate in run parameters"
                let! (selfSymRate: float<selfSymRate>) = rp.GetSelfSymRate() |> Result.ofOption "Missing selfSymRate in run parameters"
                let! modificationRate = rp.GetModificationRate() |> Result.ofOption "Missing modificationRate."

                let sorterModelMutator = SimpleSorterModelMutator.getMsrsModelMutator
                                                (RngFactory.create rngType)
                                                ExcludeSelfCe
                                                modificationRate
                                                orthoRate
                                                paraRate
                                                selfSymRate
                                         |> sorterModelMutator.Simple

                log "Executing SorterRunResult.runEvolutionAsync..."
                let! (runResult: sorterRunResult) = SorterRunResult.runEvolutionAsync
                                                        genFirst
                                                        (genLast - genFirst)
                                                        sorterModelMutator
                                                        sortersPerPool
                                                        sorterChildCount
                                                        sortableTest
                                                        sorterEvalType
                                                        sorterEvalMeasure
                                                        seedSorterPoolSet
                                                        cts.Token
                                                        log

                // 4. Persistence of run stats mapping results out to output streams

                let newRp = rp.WithQueryWithGenFirst (Some false)
                let! qpSorterRunResult = 
                    host.RunDb.MakeQueryParamsFromRunParams newRp (outputDataType.SorterRunResult "")
                    |> Result.ofOption "Failed to create QueryParams for SorterRunResult."

                log (sprintf "Saving SorterRunResult - Id: %s" (string qpSorterRunResult.Id))
                do! host.RunDb.saveAsync qpSorterRunResult (runResult |> outputData.SorterRunResult) allowOverwrite
                
                
                log "evaluateEvolutionRun completed."
                return newRp.WithRunFinished (Some true)

            with e -> 
                let errorMsg = sprintf "Error in evaluateEvolutionRun: %s" e.Message
                log errorMsg 
                return! Error errorMsg
        } |> Async.map (logResult progress log)


    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                evaluateEvolutionRun
                    CommonSorterSgd.makeStandardTests
                    CommonSorterSgd.createSeedSorterPoolSetStandard
                    host rp allowOverwrite cts progress }

    let mergeExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                evaluateEvolutionRun
                    CommonSorterSgd.makeMergeTests
                    CommonSorterSgd.createSeedSorterPoolSetMerge
                    host rp allowOverwrite cts progress }

    let fullReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                CommonSorterSgd.makeFullReport
                    host rp allowOverwrite cts progress }



    let getExecutor (executorType: sorterSgdExecutorType) : IRunParamsExecutor =
        match executorType with
        | sorterSgdExecutorType.GenStandard -> standardExecutor
        | sorterSgdExecutorType.GenMerge -> mergeExecutor
        | sorterSgdExecutorType.FullReport -> fullReportExecutor
