namespace GeneSort.Dispatch.V1.SorterEval

open System
open FSharp.UMX
open GeneSort.Core
open GeneSort.Sorting
open GeneSort.Project.V1
open GeneSort.Model.Sorting.V1

module SorterModelSetMakers =

    /// Creates and returns a full sorterModelSet using CommonSorterEval generator params.
    let makeUniformSorterModelSet (rp: runParameters) : sorterModelSet option =
        maybe {
            let! sortingWidth = rp.GetSortingWidth()
            let! simpleSorterModelType = rp.GetSimpleSorterModelType()
            let! rngType = rp.GetRngType()
            let! excludeSelfCe = rp.GetExcludeSelfCe()
            let! totalSorterCount = rp.GetSorterCount()
            let! repl = rp.GetRepl()

            let modelGen = CommonSorterEval.getSimpleUniformSorterModelGen rngType sortingWidth simpleSorterModelType excludeSelfCe
            let baseFirstIdx = (%repl * %totalSorterCount) |> UMX.tag<sorterCount>

            return SorterModelGen.makeSorterModelSetFromIndexSpan (Guid.Empty |> UMX.tag) baseFirstIdx totalSorterCount modelGen
        }