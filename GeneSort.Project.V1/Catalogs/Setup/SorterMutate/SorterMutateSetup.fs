namespace GeneSort.Project.V1.Catalogs.Setup.SorterMutate

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
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Eval.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SorterModelSet
open GeneSort.Project.V1.ParamBundles
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Catalogs.Evolution
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Runtime.OpsUtils
open GeneSort.Project.V1.Catalogs.Setup.SorterEval

module SorterMutateSetup =

    let makeMutantStandardSorterModels (sources: evolutionSetupSources) (rp:runParameters) : Async<Result<sorterModel seq, string>> =
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
                        sources.LoadStandardSorterEvals
                                            sortingWidth 
                                            simpleSorterModelType
                                            sorterEvalType.V2

            let _sorterEvalSelection = 
                            SorterSelection.makeSelection 
                                        sem 
                                        sest
                                        parentSorterSetEval.SorterEvals
                                        parentSorterSetEval.SorterTestId

            let! (parentSorterModelGen: sorterModelGen) =
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                                        rngType 
                                        sortingWidth 
                                        simpleSorterModelType
                                        excludeSelfCe
                |> SeedSorterModelGeneration.applyParameters rp


            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let! sorterModelMutator = MutatorParams.toModelMutator
                                            modificationRate
                                            mutatorPrams
                                        |> MutatorMakers.applyParameters rp

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



    let makeMutantMergeSorterModels (sources: evolutionSetupSources) (rp:runParameters) : Async<Result<sorterModel seq, string>> =
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
                        sources.LoadMergeSorterEvals
                                        mrgLibid
                                        repl
                                        simpleSorterModelType

            let _sorterEvalSelection = 
                            SorterSelection.makeSelection 
                                        sem 
                                        sest
                                        parentSorterSetEval.SorterEvals   
                                        parentSorterSetEval.SorterTestId

            let! (parentSorterModelGen: sorterModelGen) =
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                                        rngType 
                                        mrgLibid.SortingWidth 
                                        simpleSorterModelType
                                        excludeSelfCe
                |> SeedSorterModelGeneration.applyParameters rp

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let! sorterModelMutator = MutatorParams.toModelMutator
                                            modificationRate
                                            mutatorPrams
                                        |> MutatorMakers.applyParameters rp


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



    let makeMutantPrefixSorterModels (sources: evolutionSetupSources) (rp:runParameters) : Async<Result<sorterModel seq, string>> =
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
                        sources.LoadPrefixSorterEvals
                                        pfxLibid 
                                        repl
                                        simpleSorterModelType

            let _sorterEvalSelection = 
                            SorterSelection.makeSelection 
                                        sem 
                                        sest
                                        parentSorterSetEval.SorterEvals   
                                        parentSorterSetEval.SorterTestId

            let! (parentSorterModelGen: sorterModelGen) =
                SeedSorterModelGeneration.getSimpleUniformSorterModelGen 
                                        rngType 
                                        pfxLibid.SortingWidth 
                                        simpleSorterModelType
                                        excludeSelfCe
                |> SeedSorterModelGeneration.applyParameters rp

            let parentSorterModelSet = _sorterEvalSelection.MakeSorterModelSet
                                            (Guid.Empty |> UMX.tag)
                                            parentSorterModelGen

            let! sorterModelMutator = MutatorParams.toModelMutator
                                            modificationRate
                                            mutatorPrams
                                        |> MutatorMakers.applyParameters rp


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
            (storage: runStorage)
            (allowOverwrite: bool<allowOverwrite>)
            (cts: CancellationTokenSource)
            (rp: runParameters) : Async<Result<unit, string>> =
        asyncResult {
            try
                do! checkCancellation cts.Token
                let! query =
                    storage.QueryParamsFromRunParams rp (geneSortDataType.SorterModelSet "")
                    |> Result.ofOption "Failed to create QueryParams for mutant SorterModelSet."
                return! SetupArtifacts.saveOnce storage.RunDb (SetupInputReferences.resolveRecorded rp query) (fun () -> asyncResult {
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
                    return outputData.SorterModelSet modelSet
                })
            with error ->
                return! Error (sprintf "Failed to generate or save mutant SorterModelSet: %s" error.Message)
        }

    let private bindSetup (sources: workflowSources) (configured: run) (storage: runStorage) allowOverwrite (cts: CancellationTokenSource) =
        let checkedSetup (setup: setupFunction) : setupFunction =
            fun rp -> asyncResult {
                do! checkCancellation cts.Token
                return! setup rp
            }
        let context =
            setupContext.create {
                MakeSorterModelSets = Map.ofList [
                    SorterModelSetCatalogNames.mutantStandard, storeMutantSorterModelSet (makeMutantStandardSorterModels (sources.Evolution (UMX.tag "Standard") (UMX.tag "Standard"))) storage allowOverwrite cts
                    SorterModelSetCatalogNames.mutantMerge, storeMutantSorterModelSet (makeMutantMergeSorterModels (sources.Evolution (UMX.tag "Merge") (UMX.tag "Merge"))) storage allowOverwrite cts
                    SorterModelSetCatalogNames.mutantPrefix, storeMutantSorterModelSet (makeMutantPrefixSorterModels (sources.Evolution (UMX.tag "Prefix") (UMX.tag "Prefix"))) storage allowOverwrite cts
                ]
                MakeStandardTestsWithPrefix = SortableTestsMakers.storeTests SortableTestsMakers.makeStandardTests storage allowOverwrite |> checkedSetup
                MakeMergeTestsWithPrefix = SortableTestsMakers.storeTests (SortableTestsMakers.getMergeTests (sources.Evolution (UMX.tag "Merge") (UMX.tag "Merge"))) storage allowOverwrite |> checkedSetup
                MakePrefixTestsWithPrefix = SortableTestsMakers.storeTests (SortableTestsMakers.getPrefixTests (sources.Evolution (UMX.tag "Prefix") (UMX.tag "Prefix"))) storage allowOverwrite |> checkedSetup
            }
        SetupCatalog.get (Run.requireSetupFunctionName configured) context

    let bind sources configured storage : runFunction =
        fun rp allowOverwrite cts progress ->
            let setup = bindSetup sources configured storage allowOverwrite cts
            SetupExecution.run setup rp cts progress

