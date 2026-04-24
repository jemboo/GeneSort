namespace GeneSort.Model.Sorting.V1

open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Sorting.Sorter
open GeneSort.Model.Sorting.Simple.V1


type sorterModelMutator =
     | Simple of simpleSorterModelMutator
     | Unknown


module SorterModelMutator =

    let getId (model:sorterModelMutator) : Guid<sorterModelMutatorId> =
        match model with
        | Simple ssm -> SorterModelMutator.getId ssm
        | Unknown -> failwith "Unknown sorterModel"

    let getParentSorterModel (model:sorterModelMutator) : sorterModel =
        match model with
        | Simple ssm -> SorterModelMutator.getParentSorterModel ssm
                            |> sorterModel.Simple
        | Unknown -> failwith "Unknown sorterModel"


    let getParentSorterModelId (model: sorterModelMutator) : Guid<sorterModelId> =
        match model with
        | Simple ssm -> SorterModelMutator.getParentSorterModelId ssm
        | Unknown -> failwith "Unknown sorterModel"


    let getSortingWidth (model: sorterModelMutator) : int<sortingWidth> =
        match model with
        | Simple ssm -> SorterModelMutator.getSortingWidth ssm
        | Unknown -> failwith "Unknown sorterModel"


    let getCeLength (model: sorterModelMutator) : int<ceLength> =
        match model with
        | Simple ssm -> SorterModelMutator.getCeLength ssm
        | Unknown -> failwith "Unknown sorterModel"


    let makeMutantSorterModelFromIndex 
                (index: int)  
                (model: sorterModelMutator) : sorterModel =
        match model with
        | Simple ssm -> SorterModelMutator.makeMutantSorterModelFromIndex index ssm 
                            |> sorterModel.Simple
        | Unknown -> failwith "Unknown sorterModel"


    let makeMutantSorterModelFromId 
                (sorterModelId: Guid<sorterModelId>)  
                (model: sorterModelMutator) : sorterModel =

        match model with
        | Simple ssm -> (SorterModelMutator.makeMutantSorterModelFromId sorterModelId ssm)
                         |> sorterModel.Simple
        | Unknown -> failwith "Unknown sorterModel"






