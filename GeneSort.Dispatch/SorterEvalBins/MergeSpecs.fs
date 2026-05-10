namespace GeneSort.Dispatch.V1.SorterEvalBins

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Db.V1
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1
open GeneSort.Dispatch.V1
open Yab

module MergeSpecs =


    let private mergeEnhancer (host: IRunHost) (rp: runParameters) : runParameters =
        let sw = rp.GetSortingWidth().Value
        let smt = rp.GetSimpleSorterModelType().Value
        let qp = host.MakeQueryParamsFromRunParams rp (outputDataType.RunParameters host.Run.RunName)
        let sl = getMergeStageLength smt sw
        let cl = sl |> StageLength.toCeLength sw

        rp.WithProjectName(Some host.Run.ProjectName)
          .WithRunName(Some host.Run.RunName)
          .WithRunFinished(Some false)
          .WithExcludeSelfCe(Some (true |> UMX.tag<excludeSelfCe>))
          .WithCeLength(Some cl)
          .WithStageLength(Some sl)
          .WithCollectSortableTests(Some true)
          .WithId (Some qp.Id)

    let private paramMapFilter (rp: runParameters) = 
        maybe {
            let! smt = rp.GetSimpleSorterModelType()
            let! sw = rp.GetSortingWidth()
            let! md = rp.GetMergeDimension()
            let! mst = rp.GetMergeSuffixType()
        
            let has2factor = (%sw % 2 = 0)
            let isMuf4able = (MathUtils.isAPowerOfTwo %sw)
            let isMuf6able = (%sw % 3 = 0) && (MathUtils.isAPowerOfTwo (%sw / 3))

            // We bind to unit just to enforce the filter
            let! _ = 
                match smt with
                | simpleSorterModelType.Msce -> Some ()
                | simpleSorterModelType.Mssi | simpleSorterModelType.Msrs -> 
                    if has2factor then Some () else None
                | simpleSorterModelType.Msuf4 -> 
                    if isMuf4able then Some () else None
                | simpleSorterModelType.Msuf6 -> 
                    if isMuf6able then Some () else None
                | _ -> None

            // Merge dimension check: If it doesn't divide, return None to stop
            if (%sw % %md <> 0) then return! None
        
            // Suffix check: If it's NoSuffix and width > 64, return None to stop
            if (mst.IsNoSuffix && %sw > 64) then return! None
        
            return rp
        }


    module Specs =

        let P1 (executorType: executorType) : runHostSpec = {
            ProjectName = "RandomMergeSorterBins" |> UMX.tag
            RunName = sprintf @"Merge_P1_%s" (ExecutorType.toString executorType) |> UMX.tag
            RunDescription = "Merge binning for Msce/Mssi/Msrs/Msuf4"
            DataFolder = "c:\\Projects\\RandomMergeSorterBins\\Data"
            Spans = [
                (runParameters.sortingWidthKey, [32; 64] |> List.map string)
                (runParameters.simpleSorterModelTypeKey, [simpleSorterModelType.Msce; simpleSorterModelType.Mssi; simpleSorterModelType.Msrs; simpleSorterModelType.Msuf4] |> List.map SimpleSorterModelType.toString)
                (runParameters.mergeDimensionKey, [2; 4] |> List.map string)
                (runParameters.mergeSuffixTypeKey, [mergeSuffixType.NoSuffix; mergeSuffixType.VV_1] |> List.map MergeSuffixType.toString)
                (runParameters.sorterCountKey, ["1000"])
            ]
            Filter = paramMapFilter
            Enhancer = mergeEnhancer
            AllowOverwrite = false |> UMX.tag
        }

    let Configs = Map.ofList [ ("P1", Specs.P1) ]