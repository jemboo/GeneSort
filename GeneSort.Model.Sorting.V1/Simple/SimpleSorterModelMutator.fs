namespace GeneSort.Model.Sorting.Simple.V1

open FSharp.UMX
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.Model.Sorting.V1.Simple.Si
open GeneSort.Model.Sorting.V1.Simple.Rs
open GeneSort.Model.Sorting.V1.Simple.Uf4
open GeneSort.Model.Sorting.V1.Simple.Uf6
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple


type simpleSorterModelMutator =
     | SmmMsceRandMutate of msceRandMutate
     | SmmMssiRandMutate of mssiRandMutate
     | SmmMsrsRandMutate of msrsRandMutate
     | SmmMsuf4RandMutate of msuf4RandMutate
     | SmmMsuf6RandMutate of msuf6RandMutate


module SimpleSorterModelMutator =

    let getId (model:simpleSorterModelMutator) : Guid<sorterModelMutatorId> =
        match model with
        | SmmMsceRandMutate msce -> msce.Id
        | SmmMssiRandMutate mssi -> mssi.Id
        | SmmMsrsRandMutate msrs -> msrs.Id
        | SmmMsuf4RandMutate msuf4 -> msuf4.Id
        | SmmMsuf6RandMutate msuf6 -> msuf6.Id


    let makeMutantSorterModelFromIndex 
                (mutatorModel: simpleSorterModelMutator) 
                (parentModel: simpleSorterModel)
                (index: int)  : simpleSorterModel =
        match (mutatorModel, parentModel) with
        | (SmmMsceRandMutate msce, Msce parent) -> msce.MakeSorterModelFromIndex parent index |> simpleSorterModel.Msce
        | (SmmMssiRandMutate mssi, Mssi parent) -> mssi.MakeSorterModelFromIndex parent index |> simpleSorterModel.Mssi
        | (SmmMsrsRandMutate msrs, Msrs parent) -> msrs.MakeSorterModelFromIndex parent index |> simpleSorterModel.Msrs
        | (SmmMsuf4RandMutate msuf4, Msuf4 parent) -> msuf4.MakeSorterModelFromIndex parent index |> simpleSorterModel.Msuf4
        | (SmmMsuf6RandMutate msuf6, Msuf6 parent) -> msuf6.MakeSorterModelFromIndex parent index |> simpleSorterModel.Msuf6
        | _ -> failwith "Mutator and parent model types do not match."


    let makeMutantSorterModelFromId  
                (mutatorModel: simpleSorterModelMutator) 
                (parentModel: simpleSorterModel)
                (sorterModelId: Guid<sorterModelId>) : simpleSorterModel =

        match (mutatorModel, parentModel) with
        | (SmmMsceRandMutate msce, Msce parent) -> msce.MakeSorterModelFromId parent sorterModelId |> simpleSorterModel.Msce
        | (SmmMssiRandMutate mssi, Mssi parent) -> mssi.MakeSorterModelFromId parent sorterModelId |> simpleSorterModel.Mssi
        | (SmmMsrsRandMutate msrs, Msrs parent) -> msrs.MakeSorterModelFromId parent sorterModelId |> simpleSorterModel.Msrs
        | (SmmMsuf4RandMutate msuf4, Msuf4 parent) -> msuf4.MakeSorterModelFromId parent sorterModelId |> simpleSorterModel.Msuf4
        | (SmmMsuf6RandMutate msuf6, Msuf6 parent) -> msuf6.MakeSorterModelFromId parent sorterModelId |> simpleSorterModel.Msuf6
        | _ -> failwith "Mutator and parent model types do not match."








