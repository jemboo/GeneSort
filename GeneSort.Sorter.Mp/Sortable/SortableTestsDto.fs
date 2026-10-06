namespace GeneSort.Sorter.Mp.Sortable

open MessagePack
open GeneSort.Sorter.Sortable

[<MessagePackObject>]
type sortableTestsDto =
    | Ints of sortableIntTestsDto
    | Bools of sortableBoolTestsDto

module SortableTestsDto =

    let fromDomain (sorterTest: sortableTests) : sortableTestsDto =
        match sorterTest with
        | sortableTests.Ints intTest -> Ints (SortableIntTestsDto.fromDomain intTest)
        | sortableTests.Bools boolTest -> Bools (SortableBoolTestsDto.fromDomain boolTest)

    let toDomain (dto: sortableTestsDto) : sortableTests =
        match dto with
        | Ints intTestDto -> sortableTests.Ints (SortableIntTestsDto.toDomain intTestDto)
        | Bools boolTestDto -> sortableTests.Bools (SortableBoolTestsDto.toDomain boolTestDto)