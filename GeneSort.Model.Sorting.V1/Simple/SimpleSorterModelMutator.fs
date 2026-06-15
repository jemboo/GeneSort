namespace GeneSort.Model.Sorting.Simple.V1

open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1.Simple.Ce
open GeneSort.Model.Sorting.V1.Simple.Si
open GeneSort.Model.Sorting.V1.Simple.Rs
open GeneSort.Model.Sorting.V1.Simple.Uf4
open GeneSort.Model.Sorting.V1.Simple.Uf6
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


    let getMsceModelMutator
                (rngFactory: rngFactory)
                (excludeSelfCe: bool<excludeSelfCe>)
                (modificationRate: float<modificationRate>)
                (mutationRate: float<mutationRate>)
                (insertionRate: float<insertionRate>)
                (deletionRate: float<deletionRate>) : simpleSorterModelMutator =
            msceRandMutate.create rngFactory 
                    (indelRates.createMod 
                            (%modificationRate, %mutationRate, %insertionRate, %deletionRate)
                    )
                    (%excludeSelfCe)
            |> simpleSorterModelMutator.SmmMsceRandMutate


    let getMssiModelMutator
                (rngFactory: rngFactory)
                (excludeSelfCe: bool<excludeSelfCe>)
                (modificationRate: float<modificationRate>)
                (orthoRate: float<orthoRate>)
                (paraRate: float<paraRate>) :simpleSorterModelMutator =

            mssiRandMutate.create rngFactory 
                    (opActionRates.createMod (%modificationRate, %orthoRate, %paraRate))
            |> simpleSorterModelMutator.SmmMssiRandMutate


    let getMsrsModelMutator 
                (rngFactory: rngFactory)
                (excludeSelfCe: bool<excludeSelfCe>)
                (modificationRate: float<modificationRate>)
                (orthoRate: float<orthoRate>)
                (paraRate: float<paraRate>) 
                (symRate: float<selfSymRate>) :simpleSorterModelMutator =
            msrsRandMutate.create rngFactory 
                    (opsActionRates.createMod (%modificationRate, %orthoRate, %paraRate, %symRate))
            |> simpleSorterModelMutator.SmmMsrsRandMutate


    let getMsuf4ModelMutator 
                (sortingWidth: int<sortingWidth>)
                (rngFactory: rngFactory)
                (excludeSelfCe: bool<excludeSelfCe>)
                (seedModificationRate: float<seedModificationRate>)
                (modificationRate: float<modificationRate>)
                (orthoRate: float<orthoRate>)
                (paraRate: float<paraRate>) 
                (symRate: float<selfSymRate>) :simpleSorterModelMutator =

            let opsSeedActionRates = 
                opsActionRates.createMod (%seedModificationRate, %orthoRate, %paraRate, %symRate)
            let opsActionRates = 
                opsActionRates.createMod (%modificationRate, %orthoRate, %paraRate, %symRate)
            let opsSeedTransitionRates = 
                opsTransitionRates.createUniform2 opsSeedActionRates
            let opsTransitionRates = 
                opsTransitionRates.createUniform2 opsActionRates

            msuf4RandMutate.create rngFactory 
                    (Uf4MutationRates.makeUniform2 
                                    (%sortingWidth) 
                                    opsSeedTransitionRates 
                                    opsTransitionRates )
            |> simpleSorterModelMutator.SmmMsuf4RandMutate




    let makeMutantSorterModelIdFromIndex 
                (mutatorModel: simpleSorterModelMutator) 
                (parentModel: simpleSorterModel)
                (index: int)  : Guid<sorterModelId> =
        match (mutatorModel, parentModel) with
        | (SmmMsceRandMutate msce, Msce parent) -> msce.MakeSorterModelId parent index
        | (SmmMssiRandMutate mssi, Mssi parent) -> mssi.MakeSorterModelId parent index
        | (SmmMsrsRandMutate msrs, Msrs parent) -> msrs.MakeSorterModelId parent index
        | (SmmMsuf4RandMutate msuf4, Msuf4 parent) -> msuf4.MakeSorterModelId parent index
        | (SmmMsuf6RandMutate msuf6, Msuf6 parent) -> msuf6.MakeSorterModelId parent index
        | _ -> failwith "Mutator and parent model types do not match."


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


    // Helper to extract the ID from a parent model
    let private getParentId (parentModel: simpleSorterModel) : Guid<sorterModelId> =
        match parentModel with
        | Msce parent  -> parent.Id
        | Mssi parent  -> parent.Id
        | Msrs parent  -> parent.Id
        | Msuf4 parent -> parent.Id
        | Msuf6 parent -> parent.Id


    // creates a map from mutant sorterModeIds to their parent sorterModelIds for a given list of mutant sorterModelIds,
    // and a count of how many mutants were generated from each parent. [| 0 .. (count -1)|] are the indices used to 
    // generate mutant sorterModelIds from each of the parent sorterModelIds.
    let makeMutantIdToParentIdMap 
                (mutatorModel: simpleSorterModelMutator) 
                (parentModels: simpleSorterModel [])
                (count: int) : Map<Guid<sorterModelId>, Guid<sorterModelId>> =
        
        parentModels
        |> Array.collect (fun parentModel ->
            let parentId = getParentId parentModel
            Array.init count (fun index ->
                let mutantId = makeMutantSorterModelIdFromIndex mutatorModel parentModel index
                (mutantId, parentId)
            )
        )
        |> Map.ofArray

