namespace GeneSort.Model.Sorting.V1

open FSharp.UMX

open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Model.Sorting.V1.Simple


type sorterModelGen =
     | Simple of simpleSorterModelGen
     | SimpleR of simpleSorterModelRGen
     | SimpleS of simpleSorterModelSGen
     | Unknown


module SorterModelGen =

    let getId (model:sorterModelGen) : Guid<sorterModelGenId> =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.getId ssmg
        | SimpleR ssmg -> ssmg.Id
        | SimpleS ssmg -> ssmg.Id
        | Unknown -> failwith "Unknown sorterModelGen"


    let getSortingWidth (model: sorterModelGen) : int<sortingWidth> =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.getSortingWidth ssmg
        | SimpleR ssmg -> ssmg.SortingWidth
        | SimpleS ssmg -> ssmg.SortingWidth
        | Unknown -> failwith "Unknown sorterModelGen"


    let getCeLength (model: sorterModelGen) : int<ceLength> =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.getCeLength ssmg
        | SimpleR ssmg -> ssmg.CeLength
        | SimpleS ssmg -> ssmg.CeLength
        | Unknown -> failwith "Unknown sorterModelGen"


    let makeSorterModelFromIndex (index: int<sortingGenIndex>)  (model: sorterModelGen) : sorterModel =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.makeSorterModelFromIndex index ssmg |> sorterModel.Simple
        | SimpleR ssmg -> ssmg.MakeSorterModelFromIndex index |> sorterModel.SimpleR
        | SimpleS ssmg -> ssmg.MakeSorterModelFromIndex index |> sorterModel.SimpleS
        | Unknown -> failwith "Unknown sorterModelGen"


    let makeSorterModelFromId (id: Guid<sorterModelId>) (model: sorterModelGen) : sorterModel =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.makeSorterModelFromId id ssmg |> sorterModel.Simple
        | SimpleR ssmg -> ssmg.MakeSorterModelFromId id |> sorterModel.SimpleR
        | SimpleS ssmg -> ssmg.MakeSorterModelFromId id |> sorterModel.SimpleS
        | Unknown -> failwith "Unknown sorterModelGen"


    let makeSorterModelsFromIndexSpan (firstIndex:int<sorterCount>) 
                       (count: int<sorterCount>) 
                       (model: sorterModelGen) : sorterModel[] =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.makeSorterModelsFromIndexSpan firstIndex count ssmg 
                         |> Array.map sorterModel.Simple
        | SimpleR ssmg ->
            [| for i in 0 .. int count - 1 ->
                ssmg.MakeSorterModelFromIndex (UMX.tag (int firstIndex + i)) |> sorterModel.SimpleR |]
        | SimpleS ssmg ->
            [| for i in 0 .. int count - 1 ->
                ssmg.MakeSorterModelFromIndex (UMX.tag (int firstIndex + i)) |> sorterModel.SimpleS |]
        | Unknown -> failwith "Unknown sorterModelGen"


    let makeSorterModelsFromIds (ids: Guid<sorterModelId> []) (model: sorterModelGen) : sorterModel[] =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.makeSorterModelsFromIds ids ssmg |> Array.map sorterModel.Simple
        | SimpleR ssmg -> ids |> Array.map (ssmg.MakeSorterModelFromId >> sorterModel.SimpleR)
        | SimpleS ssmg -> ids |> Array.map (ssmg.MakeSorterModelFromId >> sorterModel.SimpleS)
        | Unknown -> failwith "Unknown sorterModelGen"


    let makeSorterModelSetFromIndexSpan 
                           (id: Guid<sorterModelSetId>) 
                           (firstIndex:int<sorterCount>) 
                           (count: int<sorterCount>)     
                           (modelGen: sorterModelGen)    
                           : sorterModelSet =
        let sorterModels = makeSorterModelsFromIndexSpan firstIndex count modelGen
        sorterModelSet.create id sorterModels (modelGen |> getCeLength)


    let makeSorterModelSetFromIds 
                           (id: Guid<sorterModelSetId>) 
                           (modelIds: Guid<sorterModelId> []) 
                           (modelGen: sorterModelGen)    
                           : sorterModelSet =
        let sorterModels = makeSorterModelsFromIds modelIds modelGen
        sorterModelSet.create id sorterModels (modelGen |> getCeLength)



 
