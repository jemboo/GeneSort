namespace GeneSort.Model.Sorting.V1

open FSharp.UMX
open GeneSort.Model.Sorting.Simple.V1


type sorterModelMutator =
     | Simple of simpleSorterModelMutator
     | Unknown


module SorterModelMutator =

    let getId (model:sorterModelMutator) : Guid<sorterModelMutatorId> =
        match model with
        | Simple ssm -> SimpleSorterModelMutator.getId ssm
        | Unknown -> failwith "Unknown sorterModel"


    let makeMutantSorterModelFromIndex 
                    (sorterModelMutator: sorterModelMutator) 
                    (parentModel: sorterModel)
                    (index: int<sorterMutationIndex>)  : sorterModel =

        match (sorterModelMutator, parentModel) with
        | (Simple ssm, sorterModel.Simple parent) -> 
                    SimpleSorterModelMutator.makeMutantSorterModelFromIndex 
                        ssm parent index 
                    |> sorterModel.Simple
        | _ -> failwith "Mutator and parent model types do not match."



    let makeMutantSorterModelFromId 
                    (mutatorModel: sorterModelMutator) 
                    (parentModel: sorterModel)
                    (sorterModelId: Guid<sorterModelId>) : sorterModel =
        match (mutatorModel, parentModel) with
        | (Simple ssm, sorterModel.Simple parent) -> 
                    SimpleSorterModelMutator.makeMutantSorterModelFromId ssm parent sorterModelId 
                    |> sorterModel.Simple
        | _ -> failwith "Mutator and parent model types do not match."








