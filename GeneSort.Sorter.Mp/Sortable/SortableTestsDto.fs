namespace GeneSort.Sorter.Mp.Sortable

open MessagePack
open GeneSort.Sorter.Sortable

[<MessagePackObject>]
type sortableTestsDto =
    | Ints of sortableIntTestDto
    | Bools of sortableBoolTestDto

module SortableTestsDto =

    let fromDomain (sorterTest: sortableTests) : sortableTestsDto =
        match sorterTest with
        | sortableTests.Ints intTest -> Ints (SortableIntTestDto.fromDomain intTest)
        | sortableTests.Bools boolTest -> Bools (SortableBoolTestDto.fromDomain boolTest)

    let toDomain (dto: sortableTestsDto) : sortableTests =
        match dto with
        | Ints intTestDto -> sortableTests.Ints (SortableIntTestDto.toDomain intTestDto)
        | Bools boolTestDto -> sortableTests.Bools (SortableBoolTestDto.toDomain boolTestDto)