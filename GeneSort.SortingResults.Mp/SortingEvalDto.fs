namespace GeneSort.SortingResults.Mp

open MessagePack
open GeneSort.SortingResults

[<MessagePackObject>]
type sortingEvalDto = {
    [<Key(0)>]
    Tag: int   // 0 = Single, 1 = Pairs
    [<Key(1)>]
    Single: singleSortingEvalDto option
    [<Key(2)>]
    Pairs: pairsSortingEvalDto option
}

module SortingEvalDto =

    let fromDomain (result: sortingEval) : sortingEvalDto =
        match result with
        | sortingEval.Single inner ->
            { Tag    = 0
              Single = Some (SingleSortingEvalDto.fromDomain inner)
              Pairs  = None }
        | sortingEval.Pairs inner ->
            { Tag    = 1
              Single = None
              Pairs  = Some (PairsSortingEvalDto.fromDomain inner) }

    let toDomain (dto: sortingEvalDto) : sortingEval =
        match dto.Tag with
        | 0 ->
            match dto.Single with
            | Some inner -> sortingEval.Single (SingleSortingEvalDto.toDomain inner)
            | None       -> failwith "sortingResultDto tag=0 (Single) but Single field is None"
        | 1 ->
            match dto.Pairs with
            | Some inner -> sortingEval.Pairs (PairsSortingEvalDto.toDomain inner)
            | None       -> failwith "sortingResultDto tag=1 (Pairs) but Pairs field is None"
        | n -> failwithf "Unknown sortingResult tag: %d" n