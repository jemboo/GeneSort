namespace GeneSort.Model.Mp.Sorting
open System

open FSharp.UMX
open MessagePack

open GeneSort.Model.Sorting
open GeneSort.Sorting

[<MessagePackObject>]
type sortingMutationSegmentDto = {
    [<Key(0)>]
    Id: Guid
    [<Key(1)>]
    SortingMutator: sortingMutatorDto
    [<Key(2)>]
    FirstIndex: int
    [<Key(3)>]
    Count: int
}

module SortingMutationSegmentDto =

    let fromDomain (seg: sortingMutationSegment) : sortingMutationSegmentDto =
        {
            Id             = %seg.Id
            SortingMutator = SortingMutatorDto.fromDomain seg.SortingMutator
            FirstIndex     = %seg.FirstIndex
            Count          = %seg.Count
        }

    let toDomain (dto: sortingMutationSegmentDto) : sortingMutationSegment =
        let mutator    = SortingMutatorDto.toDomain dto.SortingMutator
        let firstIndex = UMX.tag<sorterCount> dto.FirstIndex
        let count      = UMX.tag<sorterCount> dto.Count
        sortingMutationSegment.create mutator firstIndex count

    let serialize (options: MessagePackSerializerOptions)
                  (seg: sortingMutationSegment) : byte array =
        seg
        |> fromDomain
        |> fun dto -> MessagePackSerializer.Serialize(dto, options)

    let deserialize (options: MessagePackSerializerOptions)
                    (data: byte array) : sortingMutationSegment =
        MessagePackSerializer.Deserialize<sortingMutationSegmentDto>(data, options)
        |> toDomain