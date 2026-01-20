namespace GeneSort.Sorter.Mp.Sortable

open MessagePack
open GeneSort.Sorter.Sortable

[<MessagePackObject>]
type sortableTestsDto =
    | Ints of sortableIntTestDto
    | Bools of sortableBoolTestDto

module SortableTestsDto =

    let fromDomain (sorterTest: sortableTest) : sortableTestsDto =
        match sorterTest with
        | sortableTest.Ints intTest -> Ints (SortableIntTestDto.fromDomain intTest)
        | sortableTest.Bools boolTest -> Bools (SortableBoolTestDto.fromDomain boolTest)

    let toDomain (dto: sortableTestsDto) : sortableTest =
        match dto with
        | Ints intTestDto -> sortableTest.Ints (SortableIntTestDto.toDomain intTestDto)
        | Bools boolTestDto -> sortableTest.Bools (SortableBoolTestDto.toDomain boolTestDto)