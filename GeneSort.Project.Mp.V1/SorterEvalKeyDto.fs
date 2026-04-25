namespace GeneSort.Eval.Project.Mp.V1

open MessagePack
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Eval.V1.Bins

[<MessagePackObject>]
type SorterEvalKeyDto = {
    [<Key(0)>] CeCount: int
    [<Key(1)>] StageLength: int
}

module SorterEvalKeyMapping =

    let toDto (domain: sorterEvalKey) : SorterEvalKeyDto =
        {
            CeCount = UMX.untag domain.CeCount
            StageLength = UMX.untag domain.StageLength
        }

    let fromDto (dto: SorterEvalKeyDto) : sorterEvalKey =
        sorterEvalKey.create 
            (UMX.tag<ceLength> dto.CeCount) 
            (UMX.tag<stageLength> dto.StageLength)