namespace GeneSort.Eval.Mp.V1

open MessagePack
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Eval.V1.Bins

[<MessagePackObject>]
type sorterEvalKeyDto = {
    [<Key(0)>] CeCount: int
    [<Key(1)>] StageLength: int
}

module SorterEvalKeyDto =

    let toDto (domain: sorterEvalKey) : sorterEvalKeyDto =
        {
            CeCount = UMX.untag domain.CeCount
            StageLength = UMX.untag domain.StageLength
        }

    let fromDto (dto: sorterEvalKeyDto) : sorterEvalKey =
        sorterEvalKey.create 
            (UMX.tag<ceLength> dto.CeCount) 
            (UMX.tag<stageLength> dto.StageLength)