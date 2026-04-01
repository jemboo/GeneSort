namespace GeneSort.SortingResults.Mp.Bins

open System
open MessagePack
open FSharp.UMX
open GeneSort.SortingResults.Bins
open GeneSort.Model.Sorting

[<MessagePackObject>]
type mutationSegmentEvalBinEntryDto = {
    [<Key(0)>]
    sortingId: Guid
    [<Key(1)>]
    bins: mutationSegmentEvalBinsDto
}

[<MessagePackObject>]
type mutationSegmentEvalBinsSetDto = {
    [<Key(0)>]
    entries: mutationSegmentEvalBinEntryDto array
}

module MutationSegmentEvalBinsSetDto =

    /// Maps the Domain Dictionary to a DTO Array
    let fromDomain (domain: mutationSegmentEvalBinsSet) : mutationSegmentEvalBinsSetDto =
        // We access the internal dictionary via a sequence cast if private, 
        // but typically we'd add a member to the domain or use reflection/internal access.
        // Assuming the module has access to the private record field:
        let entries = 
            domain.GetBinDict() // Suggestion: Add a member to domain to expose Seq<KeyValuePair>
            |> Seq.map (fun kvp -> 
                { 
                    sortingId = %kvp.Key
                    bins = MutationSegmentEvalBinsDto.fromDomain kvp.Value 
                })
            |> Seq.toArray
        { entries = entries }

    /// Reconstructs the Domain Set from the DTO
    let toDomain (dto: mutationSegmentEvalBinsSetDto) : mutationSegmentEvalBinsSet =
        dto.entries
        |> Array.map (fun e -> 
            (UMX.tag<sortingId> e.sortingId, MutationSegmentEvalBinsDto.toDomain e.bins))
        |> MutationSegmentEvalBinsSet.makeFromStorage

    let serialize (options: MessagePackSerializerOptions)
                  (domain: mutationSegmentEvalBinsSet) : byte array =
        domain
        |> fromDomain
        |> fun dto -> MessagePackSerializer.Serialize(dto, options)

    let deserialize (options: MessagePackSerializerOptions)
                    (data: byte array) : mutationSegmentEvalBinsSet =
        MessagePackSerializer.Deserialize<mutationSegmentEvalBinsSetDto>(data, options)
        |> toDomain