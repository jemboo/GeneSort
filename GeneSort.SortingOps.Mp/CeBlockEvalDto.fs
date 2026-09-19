namespace GeneSort.SortingOps.Mp

open FSharp.UMX
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Sorting.Mp.Sortable

type ceBlockEvalDto = {
    Prefix: ceBlockDto
    CeBlock: ceBlockDto
    CeUseCounts: int array
    UnsortedCount: int
    SortableTest: sortableTestDto option
}

module CeBlockEvalDto =

    let fromDomain (eval: ceBlockEval) : ceBlockEvalDto =
        {
            Prefix = CeBlockDto.toCeBlockDto eval.Prefix
            CeBlock = CeBlockDto.toCeBlockDto eval.CeBlock
            // We store the raw array from the container
            CeUseCounts = eval.UseCountArray
            UnsortedCount = %eval.UnsortedCount
            SortableTest = 
                eval.SortableTest
                |> Option.map SortableTestDto.fromDomain
        }

    let toDomain (dto: ceBlockEvalDto) : ceBlockEval =
        let prefix = CeBlockDto.fromCeBlockDto dto.Prefix
        let ceb = CeBlockDto.fromCeBlockDto dto.CeBlock
        let counts = ceUseCounts.CreateFromArray dto.CeUseCounts
        let tests = dto.SortableTest |> Option.map SortableTestDto.toDomain
        let unsortedCount = dto.UnsortedCount |> UMX.tag<sortableCount>
        
        // The factory handles recreating the Lazy usedCes and stageSequence
        ceBlockEval.create prefix ceb counts unsortedCount tests