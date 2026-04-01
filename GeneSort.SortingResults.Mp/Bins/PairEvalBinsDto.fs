namespace GeneSort.SortingResults.Mp.Bins

open MessagePack
open GeneSort.SortingResults.Bins

[<MessagePackObject>]
type pairSortingEvalBinsDto = {
    [<Key(0)>]
    splitPairs: splitPairSortingEvalBinsDto option
    [<Key(1)>]
    splitPairs2: splitPairSortingEvalBinsDto option
}

module PairSortingEvalBinsDto =

    /// Maps the Domain Discriminated Union to the DTO
    let fromDomain (domain: pairSortingEvalBins) : pairSortingEvalBinsDto =
        match domain with
        | SplitPairs sp -> 
            { splitPairs = Some (SplitPairSortingEvalBinsDto.fromDomain sp); splitPairs2 = None }
        | SplitPairs2 sp2 -> 
            { splitPairs = None; splitPairs2 = Some (SplitPairSortingEvalBinsDto.fromDomain sp2) }

    /// Maps the DTO back to the Domain Discriminated Union
    let toDomain (dto: pairSortingEvalBinsDto) : pairSortingEvalBins =
        match dto.splitPairs, dto.splitPairs2 with
        | Some sp, None -> SplitPairs (SplitPairSortingEvalBinsDto.toDomain sp)
        | None, Some sp2 -> SplitPairs2 (SplitPairSortingEvalBinsDto.toDomain sp2)
        | _ -> failwith "Invalid pairSortingEvalBinsDto state: exactly one case must be present."

    let serialize (options: MessagePackSerializerOptions)
                  (domain: pairSortingEvalBins) : byte array =
        domain
        |> fromDomain
        |> fun dto -> MessagePackSerializer.Serialize(dto, options)

    let deserialize (options: MessagePackSerializerOptions)
                    (data: byte array) : pairSortingEvalBins =
        MessagePackSerializer.Deserialize<pairSortingEvalBinsDto>(data, options)
        |> toDomain