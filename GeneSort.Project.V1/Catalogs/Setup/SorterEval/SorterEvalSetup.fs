namespace GeneSort.Project.V1.Catalogs.Setup.SorterEval

open System
open System.Threading
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.V1
open GeneSort.SortingOps
open GeneSort.Project.V1
open GeneSort.Project.V1.Catalogs.Setup
open GeneSort.Project.V1.Catalogs.SorterModelSet
open GeneSort.Project.V1.Parameters
open GeneSort.Project.V1.Runtime
open GeneSort.Project.V1.Storage
open GeneSort.Project.V1.Runtime.OpsUtils



module SorterEvalSetup =

    let private bindSetup (sources: workflowSources) (configured: run) (storage: runStorage) allowOverwrite (cts: CancellationTokenSource) =
        let checkedSetup (setup: setupFunction) : setupFunction =
            fun rp -> asyncResult {
                do! checkCancellation cts.Token
                return! setup rp
            }
        let context =
            setupContext.create {
                MakeSorterModelSets = Map.ofList [
                    SorterModelSetCatalogNames.uniform, SorterModelSetMakers.makeUniformSorterModelSet storage allowOverwrite |> checkedSetup
                ]
                MakeStandardTestsWithPrefix = SortableTestsMakers.storeTests SortableTestsMakers.makeStandardTests storage allowOverwrite |> checkedSetup
                MakeMergeTestsWithPrefix = SortableTestsMakers.storeTests (SortableTestsMakers.getMergeTests sources.Evolution) storage allowOverwrite |> checkedSetup
                MakePrefixTestsWithPrefix = SortableTestsMakers.storeTests (SortableTestsMakers.getPrefixTests sources.Evolution) storage allowOverwrite |> checkedSetup
            }
        SetupCatalog.get (Run.requireSetupFunctionName configured) context


    /// Uses the run's setup catalog for Standard, Merge, or Prefix setup.
    let bind sources configured storage : runFunction =
        fun rp allowOverwrite cts progress ->
            let setup = bindSetup sources configured storage allowOverwrite cts
            SetupExecution.run setup rp cts progress

