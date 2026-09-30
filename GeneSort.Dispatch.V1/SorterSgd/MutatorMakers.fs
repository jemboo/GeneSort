
namespace GeneSort.Dispatch.V1.SorterSgd

open GeneSort.Model.Sorting.V1
open GeneSort.Project.V1
open FsToolkit.ErrorHandling
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Model.Sorting.Simple.V1.SimpleSorterModelMutator
open GeneSort.Core

module MutatorMakers =

    let makeSimpleSorterModelMutator (rp: runParameters) : Async<Result<simpleSorterModelMutator, string>> =
            async {
                let paramsOpt = option {
                    let! sortingWidth = rp.GetSortingWidth()
                    let! rngType = rp.GetRngType()
                    let! simpleSorterModelType = rp.GetSimpleSorterModelType()
                    return (sortingWidth, rngType, simpleSorterModelType)
                }

                match paramsOpt with
                | Some (sortingWidth, rngType, simpleSorterModelType) ->
                    let rngFactory = RngFactory.create rngType

                    match simpleSorterModelType with
                    | simpleSorterModelType.Msce ->
                        let res = option {
                            let! excludeSelfCe = rp.GetExcludeSelfCe()
                            let! modificationRate = rp.GetModificationRate()
                            let! mutationRate = rp.GetMutationRate()
                            let! insertionRate = rp.GetInsertionRate()
                            let! deletionRate = rp.GetDeletionRate()
                            let! mutVariant = rp.GetMutatorVariant()
                            return getMsceModelMutator rngFactory excludeSelfCe modificationRate mutationRate insertionRate deletionRate mutVariant
                        }
                        match res with
                        | Some mutator -> return Ok mutator
                        | None -> return Error "Failed: Missing run parameters for Msce model mutator."

                    | simpleSorterModelType.Mssi ->
                        let res = option {
                            let! excludeSelfCe = rp.GetExcludeSelfCe()
                            let! modificationRate = rp.GetModificationRate()
                            let! orthoRate = rp.GetOrthoRate()
                            let! paraRate = rp.GetParaRate()
                            let! mutVariant = rp.GetMutatorVariant()
                            return getMssiModelMutator rngFactory excludeSelfCe modificationRate orthoRate paraRate mutVariant
                        }
                        match res with
                        | Some mutator -> return Ok mutator
                        | None -> return Error "Failed: Missing run parameters for Mssi model mutator."

                    | simpleSorterModelType.Msrs ->
                        let res = option {
                            let! excludeSelfCe = rp.GetExcludeSelfCe()
                            let! modificationRate = rp.GetModificationRate()
                            let! orthoRate = rp.GetOrthoRate()
                            let! paraRate = rp.GetParaRate()
                            let! selfSymRate = rp.GetSelfSymRate()
                            let! mutVariant = rp.GetMutatorVariant()
                            return getMsrsModelMutator rngFactory excludeSelfCe modificationRate orthoRate paraRate selfSymRate mutVariant
                        }
                        match res with
                        | Some mutator -> return Ok mutator
                        | None -> return Error "Failed: Missing run parameters for Msrs model mutator."

                    | simpleSorterModelType.Msuf4 ->
                        let res = option {
                            let! excludeSelfCe = rp.GetExcludeSelfCe()
                            let! seedModificationRate = rp.GetSeedModificationRate()
                            let! modificationRate = rp.GetModificationRate()
                            let! orthoRate = rp.GetOrthoRate()
                            let! paraRate = rp.GetParaRate()
                            let! selfSymRate = rp.GetSelfSymRate()
                            let! mutVariant = rp.GetMutatorVariant()
                            return getMsuf4ModelMutator sortingWidth rngFactory excludeSelfCe seedModificationRate modificationRate orthoRate paraRate selfSymRate mutVariant
                        }
                        match res with
                        | Some mutator -> return Ok mutator
                        | None -> return Error "Failed: Missing run parameters for Msuf4 model mutator."

                    | simpleSorterModelType.Msuf6 ->
                        let res = option {
                            let! excludeSelfCe = rp.GetExcludeSelfCe()
                            let! seedModificationRate = rp.GetSeedModificationRate()
                            let! modificationRate = rp.GetModificationRate()
                            let! orthoRate = rp.GetOrthoRate()
                            let! paraRate = rp.GetParaRate()
                            let! selfSymRate = rp.GetSelfSymRate()
                            let! mutVariant = rp.GetMutatorVariant()
                            return getMsuf6ModelMutator sortingWidth rngFactory excludeSelfCe seedModificationRate modificationRate orthoRate paraRate selfSymRate mutVariant
                        }
                        match res with
                        | Some mutator -> return Ok mutator
                        | None -> return Error "Failed: Missing run parameters for Msuf6 model mutator."

                | None ->
                    return Error "Failed: One or more RunParameters for StandardTests were missing."
            }
