namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.FileDb.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Model.Sortable.V1
open GeneSort.Dispatch.V1


module RandomStandard =

    let private standardEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let sw = rp.GetSortingWidth().Value
        let qp = host.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters %host.Project.ProjectName)
        
        // Note: Logic previously using spec.GetStageLength can be moved here or 
        // calculated if standardStageLength is accessible.
        let sl = match %sw with | 12 -> 40 |> UMX.tag | _ -> 40 |> UMX.tag
        let cl = sl |> StageLength.toCeLength sw
        
        rp.WithProjectName(Some host.Project.ProjectName)
          .WithRunName(Some host.Project.RunName)
          .WithRunFinished(Some false)
          .WithCeLength(Some cl)
          .WithStageLength(Some sl)
          .WithId (Some qp.Id)

    let private standardSorterModelTypeFilter (rp: runParameters) =
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let has2factor = (%sw % 2 = 0)
            return! if (smt = simpleSorterModelType.Msce) || has2factor then Some rp else None
        }

    module Specs =
        let P1 = {
            ProjectName = "SorterEvalBins" |> UMX.tag
            RunName = "Dev" |> UMX.tag
            ProjectDesc = "Standard binning for Msce/Mssi/Msrs"
            DataFolder = "c:\\ProjectsV1\\SorterEvalBins\\RandomStandard\\Data"
            Spans = [
                (runParameters.sortingWidthKey, 
                ["12"])
                (runParameters.simpleSorterModelTypeKey, 
                [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)
                (runParameters.sortableDataFormatKey, 
                [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (runParameters.sorterCountKey, 
                ["1000"])
            ]
            GetStageLength = fun sw -> (match %sw with | 12 -> 40 | _ -> 40) |> UMX.tag
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            RngFactory = rngFactory.LcgFactory
            CollectNewSortableTests = true
            AllowOverwrite = false |> UMX.tag
            TestModelFactory = fun rp -> msasF.create (rp.GetSortingWidth().Value) |> sortableTestModel.MsasF
            SorterModelGenFactory = fun rp -> 
                SimpleSorterModelGen.makeUniform rngFactory.LcgFactory 
                    (rp.GetSortingWidth().Value) (rp.GetStageLength().Value) (rp.GetSimpleSorterModelType().Value) 
                |> sorterModelGen.Simple
        }

        let P2 = {
            ProjectName = "SorterEvalBins" |> UMX.tag
            RunName = "Prod" |> UMX.tag
            ProjectDesc = "Standard binning for Msce/Mssi/Msrs"
            DataFolder = "c:\\ProjectsV1\\SorterEvalBins\\RandomStandard\\Data"
            Spans = [
                (runParameters.sortingWidthKey, 
                ["12"])
                (runParameters.simpleSorterModelTypeKey, 
                [simpleSorterModelType.Msce] |> List.map SimpleSorterModelType.toString)
                (runParameters.sortableDataFormatKey, 
                [sortableDataFormat.BitVector512] |> List.map SortableDataFormat.toString)
                (runParameters.sorterCountKey, 
                ["1000"])
            ]
            GetStageLength = fun sw -> (match %sw with | 12 -> 40 | _ -> 40) |> UMX.tag
            Filter = standardSorterModelTypeFilter
            Enhancer = standardEnhancer
            RngFactory = rngFactory.LcgFactory
            CollectNewSortableTests = true
            AllowOverwrite = false |> UMX.tag
            TestModelFactory = fun rp -> msasF.create (rp.GetSortingWidth().Value) |> sortableTestModel.MsasF
            SorterModelGenFactory = fun rp -> 
                SimpleSorterModelGen.makeUniform rngFactory.LcgFactory 
                    (rp.GetSortingWidth().Value) (rp.GetStageLength().Value) (rp.GetSimpleSorterModelType().Value) 
                |> sorterModelGen.Simple
        }


    let Configs = Map.ofList [ ("P1", Specs.P1); ("P2", Specs.P2) ]

    let CreateHost (spec: runHostSpec) =
        let folder = spec.DataFolder |> UMX.tag
        let db = new GeneSortDbMp(folder) :> IGeneSortDb
        let proj = run.create spec.ProjectName spec.RunName spec.ProjectDesc 
                                [| outputDataType.RunParameters %spec.ProjectName; outputDataType.SorterEvalBins ""; |]
        runHost.Create db spec proj :> IRunHost
