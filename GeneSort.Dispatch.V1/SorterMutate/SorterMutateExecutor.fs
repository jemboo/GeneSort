namespace GeneSort.Dispatch.V1.SorterMutate

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.SortingLib.Sorter
open GeneSort.SortingOps
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SorterModelSet
open GeneSort.Project.V1.ParamBundles
open GeneSort.Project.V1.Parameters
open GeneSort.Db.V1
open GeneSort.Dispatch.V1
open GeneSort.Dispatch.V1.OpsUtils
open GeneSort.Dispatch.V1.SorterEval
open GeneSort.Model.Sorting.Simple.V1


module SorterMutateExecutor =

    /// Runs the bound setup catalog without evaluating mutants or completing the run.
    let _makeSetup = SorterEvalExecutor._makeSetup

    let makeMutantStandardSorterModels (rp:runParameters) : Async<Result<sorterModel seq, string>> =
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

            let! (mutatorPrams: mutatorParams ) =  
                        rp.GetMutatorParams()
                        |> Result.ofOption "Missing mutatorParams in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            let! (sest: sorterSelectionType) = 
                        rp.GetSeedSorterPoolSelectionType()
                        |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"

            let! (sem:sorterEvalMeasure) = 
                        rp.GetSorterEvalMeasure()
                        |> Result.ofOption "Missing sorterEvalMeasure in run parameters"

            let! (mutationMod: int<mutationMod>) = 
                        rp.GetMutationMod() 
                        |> Result.ofOption "Missing mutationMod in run parameters"

            let! (excludeSelfCe: bool<excludeSelfCe>) = 
                        rp.GetExcludeSelfCe()
                        |> Result.ofOption "Missing excludeSelfCe in run parameters"

            let! (mutVariant: mutatorVariant) = 
                        rp.GetMutatorVariant()
                        |> Result.ofOption "Missing mutatorVariant in run parameters"

            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getStandardSorterEvals 
                                            sortingWidth 
                                            simpleSorterModelType
                                            sorterEvalType.V2

            let _sorterEvalSelection = 
                            SorterSelection.makeSelection 
                                        sem 
                                        sest
                                        parentSorterSetEval.SorterEvals
                                        parentSorterSetEval.SorterTestId

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        sortingWidth 
                                        simpleSorterModelType
                                        excludeSelfCe


            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let sorterModelMutator = MutatorParams.toModelMutator
                                            modificationRate
                                            mutatorPrams

            let childIndexes = [| 0 .. (%sorterChildCount - 1) |]

            // Streaming engine via sequence expression
            let generateMutantStream (parents: sorterModel[]) =
                seq {
                    for parentModel in parents do
                        for dex in childIndexes do
                            yield SorterModelMutator.makeMutantSorterModelFromIndexAndMod
                                        sorterModelMutator
                                        parentModel
                                        (dex |> UMX.tag<mutationIndex>)
                                        mutationMod
                }

            return generateMutantStream parentSorterModelSet.SorterModels
        }



    let makeMutantMergeSorterModels (rp:runParameters) : Async<Result<sorterModel seq, string>> =
        asyncResult {

            let! (repl: int<replNumber>) = 
                        rp.GetRepl()
                        |> Result.ofOption "Missing repl in run parameters"

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (mrgLibid: mergeLibId) = 
                        rp.GetMergeLibId() 
                        |> Result.ofOption "Missing merge library ID in run parameters"
    
            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (sorterChildCount: int<sorterChildCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorterChildCount in run parameters"

            let! (mutatorPrams: mutatorParams ) =  
                        rp.GetMutatorParams()
                        |> Result.ofOption "Missing mutatorParams in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            let! (sest: sorterSelectionType) = 
                        rp.GetSeedSorterPoolSelectionType()
                        |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"

            let! (sem:sorterEvalMeasure) = 
                        rp.GetSorterEvalMeasure()
                        |> Result.ofOption "Missing sorterEvalMeasure in run parameters"

            let! (mutationMod: int<mutationMod>) = 
                        rp.GetMutationMod() 
                        |> Result.ofOption "Missing mutationMod in run parameters"

            let! (mutVariant: mutatorVariant) = 
                        rp.GetMutatorVariant()
                        |> Result.ofOption "Missing mutatorVariant in run parameters"

            let! (excludeSelfCe: bool<excludeSelfCe>) = 
                        rp.GetExcludeSelfCe()
                        |> Result.ofOption "Missing excludeSelfCe in run parameters"

            let rngFactory = rngType |> RngFactory.create

            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getMergeSorterEvals 
                                        mrgLibid
                                        repl
                                        simpleSorterModelType

            let _sorterEvalSelection = 
                            SorterSelection.makeSelection 
                                        sem 
                                        sest
                                        parentSorterSetEval.SorterEvals   
                                        parentSorterSetEval.SorterTestId

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        mrgLibid.SortingWidth 
                                        simpleSorterModelType
                                        excludeSelfCe

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let sorterModelMutator = MutatorParams.toModelMutator
                                            modificationRate
                                            mutatorPrams


            let childIndexes = [| 0 .. (%sorterChildCount - 1) |]

            // Streaming engine via sequence expression
            let generateMutantStream (parents: sorterModel[]) =
                seq {
                    for parentModel in parents do
                        for dex in childIndexes do
                            yield SorterModelMutator.makeMutantSorterModelFromIndexAndMod
                                        sorterModelMutator
                                        parentModel
                                        (dex |> UMX.tag<mutationIndex>)
                                        mutationMod
                }

            return generateMutantStream parentSorterModelSet.SorterModels
        }



    let makeMutantPrefixSorterModels (rp:runParameters) : Async<Result<sorterModel seq, string>> =
        asyncResult {

            let! (rngType: rngType) =  
                        rp.GetRngType()
                        |> Result.ofOption "Missing RNG type in run parameters"

            let! (repl: int<replNumber>) = 
                        rp.GetRepl()
                        |> Result.ofOption "Missing repl in run parameters"

            let! (pfxLibid: prefixLibId) = 
                        rp.GetPrefixLibId() 
                        |> Result.ofOption "Missing prefix library ID in run parameters"

            let! (simpleSorterModelType: simpleSorterModelType) = 
                        rp.GetSimpleSorterModelType() 
                        |> Result.ofOption "Missing simple sorter model type in run parameters"

            let! (sorterChildCount: int<sorterChildCount>) = 
                        rp.GetSorterChildCount()
                        |> Result.ofOption "Missing parent sorterChildCount in run parameters"

            let! (mutatorPrams: mutatorParams ) =  
                        rp.GetMutatorParams()
                        |> Result.ofOption "Missing mutatorParams in run parameters"

            let! (modificationRate: float<modificationRate>) =  
                        rp.GetModificationRate()
                        |> Result.ofOption "Missing modificationRate in run parameters"

            let! (sest: sorterSelectionType) = 
                        rp.GetSeedSorterPoolSelectionType()
                        |> Result.ofOption "Missing sorterEvalSelectionType in run parameters"

            let! (sem:sorterEvalMeasure) = 
                        rp.GetSorterEvalMeasure()
                        |> Result.ofOption "Missing sorterEvalMeasure in run parameters"

            let! (mutationMod: int<mutationMod>) = 
                        rp.GetMutationMod() 
                        |> Result.ofOption "Missing mutationMod in run parameters"

            let! (mutVariant: mutatorVariant) = 
                        rp.GetMutatorVariant()
                        |> Result.ofOption "Missing mutatorVariant in run parameters"


            let! (excludeSelfCe: bool<excludeSelfCe>) = 
                        rp.GetExcludeSelfCe()
                        |> Result.ofOption "Missing excludeSelfCe in run parameters"

            let rngFactory = rngType |> RngFactory.create

            let! (parentSorterSetEval: sorterSetEval) =
                        SorterEvalDbs.getPrefixSorterEvals 
                                        pfxLibid 
                                        repl
                                        simpleSorterModelType

            let _sorterEvalSelection = 
                            SorterSelection.makeSelection 
                                        sem 
                                        sest
                                        parentSorterSetEval.SorterEvals   
                                        parentSorterSetEval.SorterTestId

            let (parentSorterModelGen: sorterModelGen) = 
                CommonSorterEval.getSimpleUniformSorterModelGen 
                                        rngType 
                                        pfxLibid.SortingWidth 
                                        simpleSorterModelType
                                        excludeSelfCe

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let sorterModelMutator = MutatorParams.toModelMutator
                                            modificationRate
                                            mutatorPrams


            let childIndexes = [| 0 .. (%sorterChildCount - 1) |]

            // Streaming engine via sequence expression
            let generateMutantStream (parents: sorterModel[]) =
                seq {
                    for parentModel in parents do
                        for dex in childIndexes do
                            yield SorterModelMutator.makeMutantSorterModelFromIndexAndMod
                                        sorterModelMutator
                                        parentModel
                                        (dex |> UMX.tag<mutationIndex>)
                                        mutationMod
                }

            return generateMutantStream parentSorterModelSet.SorterModels
        }




    let storeMutantSorterModelSet
            (makeModels: runParameters -> Async<Result<sorterModel seq, string>>)
            (host: runHost)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (rp: runParameters) : Async<Result<unit, string>> =
        asyncResult {
            try
                do! checkCancellation cts.Token
                let! query =
                    host.QueryParamsFromRunParams rp (geneSortDataType.SorterModelSet "")
                    |> Result.ofOption "Failed to create QueryParams for mutant SorterModelSet."
                let! modelStream = makeModels rp
                let models =
                    modelStream
                    |> Seq.map (fun model -> cts.Token.ThrowIfCancellationRequested(); model)
                    |> Seq.toArray
                let rawCeLength =
                    if Array.isEmpty models then 0<ceLength>
                    else models |> Array.map SorterModel.getCeLength |> Array.max
                let modelSet = sorterModelSet.create (UMX.tag %query.Id) models rawCeLength
                do! checkCancellation cts.Token
                return! host.RunDb.saveAsync geneSortDataMode.Setup query (outputData.SorterModelSet modelSet) allowOverwrite
            with error ->
                return! Error (sprintf "Failed to generate or save mutant SorterModelSet: %s" error.Message)
        }

    let _evaluateMutants (host: runHost) rp allowOverwrite cts progress =
        SorterMutateExecution._evaluateMutants host.ExecutionContext rp allowOverwrite cts progress

    let private bindSetup (host: runHost) allowOverwrite (cts: CancellationTokenSource) =
        let checkedSetup (setup: setupFunction) : setupFunction =
            fun rp -> asyncResult {
                do! checkCancellation cts.Token
                return! setup rp
            }
        let context =
            setupContext.create {
                MakeSorterModelSets = Map.ofList [
                    SorterModelSetCatalogNames.mutantStandard, storeMutantSorterModelSet makeMutantStandardSorterModels host allowOverwrite cts
                    SorterModelSetCatalogNames.mutantMerge, storeMutantSorterModelSet makeMutantMergeSorterModels host allowOverwrite cts
                    SorterModelSetCatalogNames.mutantPrefix, storeMutantSorterModelSet makeMutantPrefixSorterModels host allowOverwrite cts
                ]
                MakeStandardTestsWithPrefix = SortableTestsMakers.storeTests SortableTestsMakers.makeStandardTests host allowOverwrite |> checkedSetup
                MakeMergeTestsWithPrefix = SortableTestsMakers.storeTests SortableTestsMakers.getMergeTests host allowOverwrite |> checkedSetup
                MakePrefixTestsWithPrefix = SortableTestsMakers.storeTests SortableTestsMakers.getPrefixTests host allowOverwrite |> checkedSetup
            }
        host.WithSetupContext(context)

    let setupExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                let host = bindSetup host allowOverwrite cts
                _makeSetup host rp cts progress }

    let standardExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                host.ExecuteFunction host.ExecutionContext rp allowOverwrite cts progress }

    let mergeExecutor = standardExecutor

    let prefixExecutor = standardExecutor


    let mergeReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeMutantReport
                    Reporting.makeMergeMutantDetails
                    host rp allowOverwrite cts progress }

    let standardReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeMutantReport
                    Reporting.makeStandardMutantDetails
                    host rp allowOverwrite cts progress }

    let prefixReportExecutor =
        { new IRunParamsExecutor with
            member _.Execute host rp allowOverwrite cts progress =
                Reporting.makeMutantReport
                    Reporting.makePrefixMutantDetails
                    host rp allowOverwrite cts progress }
