namespace GeneSort.SortingResults.Mp

open MessagePack
open GeneSort.SortingResults

[<MessagePackObject>]
type sortingResultDto = {
    [<Key(0)>]
    Tag: int   // 0 = Single, 1 = Pairs
    [<Key(1)>]
    Single: singleSortingResultDto option
    [<Key(2)>]
    Pairs: pairsSortingResultDto option
}

module SortingResultDto =

    let toDto (result: sortingResult) : sortingResultDto =
        match result with
        | sortingResult.Single inner ->
            { Tag    = 0
              Single = Some (SingleSortingResultDto.toDto inner)
              Pairs  = None }
        | sortingResult.Pairs inner ->
            { Tag    = 1
              Single = None
              Pairs  = Some (PairsSortingResultDto.toDto inner) }

    let fromDto (dto: sortingResultDto) : sortingResult =
        match dto.Tag with
        | 0 ->
            match dto.Single with
            | Some inner -> sortingResult.Single (SingleSortingResultDto.fromDto inner)
            | None       -> failwith "sortingResultDto tag=0 (Single) but Single field is None"
        | 1 ->
            match dto.Pairs with
            | Some inner -> sortingResult.Pairs (PairsSortingResultDto.fromDto inner)
            | None       -> failwith "sortingResultDto tag=1 (Pairs) but Pairs field is None"
        | n -> failwithf "Unknown sortingResult tag: %d" n