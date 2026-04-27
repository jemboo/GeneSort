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


    let makeManyModels (firstIndex:int<sorterCount>) 
                       (count: int<sorterCount>) 
                       (model: sorterModelGen) : sorterModel[] =
        match model with
        | Simple ssmg -> SimpleSorterModelGen.makeManyModels firstIndex count ssmg |> Array.map sorterModel.Simple
        | Unknown -> failwith "Unknown sorterModelGen"


    let makeSorterModelSet (id: Guid<sorterModelSetId>) 
                           (firstIndex:int<sorterCount>) 
                           (count: int<sorterCount>)     
                           (modelGen: sorterModelGen)    
                           : sorterModelSet =
        let sorterModels = makeManyModels firstIndex count modelGen
        sorterModelSet.create id sorterModels






