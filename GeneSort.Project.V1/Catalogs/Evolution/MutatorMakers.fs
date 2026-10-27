
namespace GeneSort.Project.V1.Catalogs.Evolution

open FsToolkit.ErrorHandling
open GeneSort.Core
open GeneSort.Model.Sorting.V1
open GeneSort.Project.V1
open GeneSort.Project.V1.Parameters
open GeneSort.Model.Sorting.Simple.V1
open GeneSort.Model.Sorting.Simple.V1.SimpleSorterModelMutator
open GeneSort.Model.Sorting.V1.Simple

module MutatorMakers =

    let validateModels (path: evoPath) (models: sorterModel seq) =
        let mismatch = models |> Seq.exists (fun model ->
            match path.GetAMax(), model with
            | None, sorterModel.SimpleS _ -> path.GetActivationProbability().IsNone
            | _, _ when Map.containsKey ParamKeys.activationProbabilityKey path.Settings -> true
            | Some maximum, sorterModel.SimpleR paired -> maximum <> paired.AMax
            | None, sorterModel.SimpleR _ -> true
            | Some _, _ -> true
            | None, _ -> false)
        if mismatch then Error "AMax and the model family must match the prepared source models; changing AMax requires new seed models."
        else Ok ()

    /// Wrap the configured model mutator without changing the underlying mutation settings.
    let private applySettings settings (mutator: sorterModelMutator) =
        let probability = ParamKeys.tryGetFloat ParamKeys.activationProbabilityKey settings
        let rate = ParamKeys.tryGetFloat ParamKeys.activatorMutationRateKey settings
        let maximum = ParamKeys.tryGetInt ParamKeys.aMaxKey settings
        if Map.containsKey ParamKeys.activationProbabilityKey settings then
            if Map.containsKey ParamKeys.aMaxKey settings then Error "ActivationProbability and AMax cannot be combined."
            else
                match probability, rate, mutator with
                | Some probability, Some rate, sorterModelMutator.Simple underlying
                    when System.Double.IsFinite probability && probability >= 0.0 && probability <= 1.0
                         && System.Double.IsFinite rate && rate >= 0.0 && rate <= 1.0 ->
                    simpleSorterModelSMutator.create underlying rate |> sorterModelMutator.SimpleS |> Ok
                | _ -> Error "SimpleS requires an underlying Simple mutator and finite ActivationProbability and ActivatorMutationRate between 0 and 1."
        else
            match Map.containsKey ParamKeys.aMaxKey settings, Map.containsKey ParamKeys.activatorMutationRateKey settings with
            | false, false -> Ok mutator
            | false, true -> Error "ActivatorMutationRate requires AMax or ActivationProbability."
            | true, _ ->
                match maximum, rate, mutator with
                | Some maximum, Some rate, sorterModelMutator.Simple underlying
                    when maximum >= 0 && System.Double.IsFinite rate && rate >= 0.0 && rate <= 1.0 ->
                    simpleSorterModelRMutator.create underlying rate |> sorterModelMutator.SimpleR |> Ok
                | Some maximum, _, _ when maximum < 0 -> Error "AMax must be a nonnegative integer."
                | None, _, _ -> Error "AMax must be a nonnegative integer."
                | _ -> Error "SimpleR requires an underlying Simple mutator and a finite ActivatorMutationRate between 0 and 1."

    let applyPath (path: evoPath) mutator = applySettings path.Settings mutator

    /// Non-evolutionary mutation setup and reports still use parameter rows.
    let applyParameters (rp: runParameters) mutator = applySettings rp.ParamMap mutator

    let makeSimpleSorterModelMutator (path: evoPath) : Async<Result<simpleSorterModelMutator, string>> =
            async {
                let paramsOpt = option {
                    let! sortingWidth = path.GetSortingWidth()
                    let! rngType = path.GetRngType()
                    let! simpleSorterModelType = path.GetSimpleSorterModelType()
                    return (sortingWidth, rngType, simpleSorterModelType)
                }

                match paramsOpt with
                | Some (sortingWidth, rngType, simpleSorterModelType) ->
                    let rngFactory = RngFactory.create rngType

                    match simpleSorterModelType with
                    | simpleSorterModelType.Msce ->
                        let res = option {
                            let! excludeSelfCe = path.GetExcludeSelfCe()
                            let! modificationRate = path.GetModificationRate()
                            let! mutationRate = path.GetMutationRate()
                            let! insertionRate = path.GetInsertionRate()
                            let! deletionRate = path.GetDeletionRate()
                            let! mutVariant = path.GetMutatorVariant()
                            return getMsceModelMutator rngFactory excludeSelfCe modificationRate mutationRate insertionRate deletionRate mutVariant
                        }
                        match res with
                        | Some mutator -> return Ok mutator
                        | None -> return Error "Failed: Missing run parameters for Msce model mutator."

                    | simpleSorterModelType.Mssi ->
                        let res = option {
                            let! excludeSelfCe = path.GetExcludeSelfCe()
                            let! modificationRate = path.GetModificationRate()
                            let! orthoRate = path.GetOrthoRate()
                            let! paraRate = path.GetParaRate()
                            let! mutVariant = path.GetMutatorVariant()
                            return getMssiModelMutator rngFactory excludeSelfCe modificationRate orthoRate paraRate mutVariant
                        }
                        match res with
                        | Some mutator -> return Ok mutator
                        | None -> return Error "Failed: Missing run parameters for Mssi model mutator."

                    | simpleSorterModelType.Msrs ->
                        let res = option {
                            let! excludeSelfCe = path.GetExcludeSelfCe()
                            let! modificationRate = path.GetModificationRate()
                            let! orthoRate = path.GetOrthoRate()
                            let! paraRate = path.GetParaRate()
                            let! selfSymRate = path.GetSelfSymRate()
                            let! mutVariant = path.GetMutatorVariant()
                            return getMsrsModelMutator rngFactory excludeSelfCe modificationRate orthoRate paraRate selfSymRate mutVariant
                        }
                        match res with
                        | Some mutator -> return Ok mutator
                        | None -> return Error "Failed: Missing run parameters for Msrs model mutator."

                    | simpleSorterModelType.Msuf4 ->
                        let res = option {
                            let! excludeSelfCe = path.GetExcludeSelfCe()
                            let! seedModificationRate = path.GetSeedModificationRate()
                            let! modificationRate = path.GetModificationRate()
                            let! orthoRate = path.GetOrthoRate()
                            let! paraRate = path.GetParaRate()
                            let! selfSymRate = path.GetSelfSymRate()
                            let! mutVariant = path.GetMutatorVariant()
                            return getMsuf4ModelMutator sortingWidth rngFactory excludeSelfCe seedModificationRate modificationRate orthoRate paraRate selfSymRate mutVariant
                        }
                        match res with
                        | Some mutator -> return Ok mutator
                        | None -> return Error "Failed: Missing run parameters for Msuf4 model mutator."

                    | simpleSorterModelType.Msuf6 ->
                        let res = option {
                            let! excludeSelfCe = path.GetExcludeSelfCe()
                            let! seedModificationRate = path.GetSeedModificationRate()
                            let! modificationRate = path.GetModificationRate()
                            let! orthoRate = path.GetOrthoRate()
                            let! paraRate = path.GetParaRate()
                            let! selfSymRate = path.GetSelfSymRate()
                            let! mutVariant = path.GetMutatorVariant()
                            return getMsuf6ModelMutator sortingWidth rngFactory excludeSelfCe seedModificationRate modificationRate orthoRate paraRate selfSymRate mutVariant
                        }
                        match res with
                        | Some mutator -> return Ok mutator
                        | None -> return Error "Failed: Missing run parameters for Msuf6 model mutator."

                | None ->
                    return Error "Failed: One or more RunParameters for StandardTests were missing."
            }
