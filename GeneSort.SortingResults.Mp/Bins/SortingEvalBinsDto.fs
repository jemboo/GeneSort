namespace GeneSort.SortingResults.Mp.Bins

open MessagePack
open GeneSort.SortingResults.Bins

[<MessagePackObject>]
type sortingEvalBinsDto = {
    [<Key(0)>]
    single: singleSortingEvalBinsDto option
    [<Key(1)>]
    pairs: pairSortingEvalBinsDto option
}

module SortingEvalBinsDto =

    /// Maps the Discriminated Union to the DTO
    let fromDomain (domain: sortingEvalBins) : sortingEvalBinsDto =
        match domain with
        | Single s -> 
            { single = Some (SingleSortingEvalBinsDto.fromDomain s); pairs = None }
        | Pairs p -> 
            { single = None; pairs = Some (PairSortingEvalBinsDto.fromDomain p) }

    /// Maps the DTO back to the Discriminated Union
    let toDomain (dto: sortingEvalBinsDto) : sortingEvalBins =
        match dto.single, dto.pairs with
        | Some s, None -> Single (SingleSortingEvalBinsDto.toDomain s)
        | None, Some p -> Pairs (PairSortingEvalBinsDto.toDomain p)
        | _ -> failwith "Invalid sortingEvalBinsDto state: must have exactly one case defined."

    let serialize (options: MessagePackSerializerOptions)
                  (domain: sortingEvalBins) : byte array =
        domain
        |> fromDomain
        |> fun dto -> MessagePackSerializer.Serialize(dto, options)

    let deserialize (options: MessagePackSerializerOptions)
                    (data: byte array) : sortingEvalBins =
        MessagePackSerializer.Deserialize<sortingEvalBinsDto>(data, options)
        |> toDomain