namespace GeneSort.Model.Sorting.V1

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.Simple.V1


type sorterModelGen =
     | Simple of simpleSorterModelGen
     | Unknown



module SorterModelGen =

    let getId (model:sorterModelGen) : Guid<sorterModelGenId> =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.getId ssmg
        | Unknown -> failwith "Unknown sorterModelGen"


    let getSortingWidth (model: sorterModelGen) : int<sortingWidth> =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.getSortingWidth ssmg
        | Unknown -> failwith "Unknown sorterModelGen"


    let getCeLength (model: sorterModelGen) : int<ceLength> =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.getCeLength ssmg
        | Unknown -> failwith "Unknown sorterModelGen"


    let makeSorterModelFromIndex (index: int)  (model: sorterModelGen) : sorterModel =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.makeSorterModelFromIndex index ssmg |> sorterModel.Simple
        | Unknown -> failwith "Unknown sorterModelGen"


    let makeSorterModelFromId (id: Guid<sorterModelId>) (model: sorterModelGen) : sorterModel =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.makeSorterModelFromId id ssmg |> sorterModel.Simple
        | Unknown -> failwith "Unknown sorterModelGen"


    let makeSorterModelsFromIndexSpan (firstIndex:int<sorterCount>) 
                       (count: int<sorterCount>) 
                       (model: sorterModelGen) : sorterModel[] =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.makeSorterModelsFromIndexSpan firstIndex count ssmg |> Array.map sorterModel.Simple
        | Unknown -> failwith "Unknown sorterModelGen"


    let makeSorterModelsFromIds (ids: Guid<sorterModelId> []) (model: sorterModelGen) : sorterModel[] =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.makeSorterModelsFromIds ids ssmg |> Array.map sorterModel.Simple
        | Unknown -> failwith "Unknown sorterModelGen"

    let makeSorterModelSetFromIndexSpan 
                           (id: Guid<sorterModelSetId>) 
                           (firstIndex:int<sorterCount>) 
                           (count: int<sorterCount>)     
                           (modelGen: sorterModelGen)    
                           : sorterModelSet =
        let sorterModels = makeSorterModelsFromIndexSpan firstIndex count modelGen
        sorterModelSet.create id sorterModels


    let makeSorterModelSetFromIds 
                           (id: Guid<sorterModelSetId>) 
                           (modelIds: Guid<sorterModelId> []) 
                           (modelGen: sorterModelGen)    
                           : sorterModelSet =
        let sorterModels = makeSorterModelsFromIds modelIds modelGen
        sorterModelSet.create id sorterModels






