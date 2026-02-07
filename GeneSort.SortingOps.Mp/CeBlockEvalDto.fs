namespace GeneSort.SortingOps.Mp

open FSharp.UMX
open MessagePack
open GeneSort.SortingOps
open GeneSort.Sorting
open GeneSort.Sorting.Mp.Sortable

[<MessagePackObject>]
type ceBlockEvalDto = {
    [<Key(0)>]
    CeBlock: ceBlockDto
    [<Key(1)>]
    CeUseCounts: int array
    [<Key(2)>]
    UnsortedCount: int
    [<Key(3)>]
    SortableTest: sortableTestDto option
}

module CeBlockEvalDto =

    let fromDomain (eval: ceBlockEval) : ceBlockEvalDto =
        {
            CeBlock = CeBlockDto.toCeBlockDto eval.CeBlock
            // We store the raw array from the container
            CeUseCounts = eval.CeUseCounts.ToArray()
            UnsortedCount = %eval.UnsortedCount
            SortableTest = 
                eval.SortableTest
                |> Option.map SortableTestDto.fromDomain
        }

    let toDomain (dto: ceBlockEvalDto) : ceBlockEval =
        let ceb = CeBlockDto.fromCeBlockDto dto.CeBlock
        let counts = ceUseCounts.CreateFromArray dto.CeUseCounts
        let tests = dto.SortableTest |> Option.map SortableTestDto.toDomain
        let unsortedCount = dto.UnsortedCount |> UMX.tag<sortableCount>
        
        // The factory handles recreating the Lazy usedCes and stageSequence
        ceBlockEval.create ceb counts unsortedCount tests