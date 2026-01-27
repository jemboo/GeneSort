namespace GeneSort.Sorting.Mp.Sortable

open MessagePack
open GeneSort.Sorting.Sortable

[<MessagePackObject>]
type sortableTestDto =
    | Ints of sortableIntTestDto
    | Bools of sortableBoolTestDto
    | Uint8v256 of sortableUint8v256TestDto
    | Uint8v512 of sortableUint8v512TestDto


module SortableTestDto =

    let fromDomain (sorterTest: sortableTest) : sortableTestDto =
        match sorterTest with
        | sortableTest.Ints intTest -> Ints (SortableIntTestDto.fromDomain intTest)
        | sortableTest.Bools boolTest -> Bools (SortableBoolTestDto.fromDomain boolTest)
        | sortableTest.Uint8v256 uint8v256Test -> 
            Uint8v256 (SortableUint8v256TestDto.fromDomain uint8v256Test)
        | sortableTest.Uint8v512 uint8v512Test -> 
            Uint8v512 (SortableUint8v512TestDto.fromDomain uint8v512Test)
        | _ -> failwith "Unsupported sortableTest variant for DTO conversion."

    let toDomain (dto: sortableTestDto) : sortableTest =
        match dto with
        | Ints intTestDto -> sortableTest.Ints (SortableIntTestDto.toDomain intTestDto)
        | Bools boolTestDto -> sortableTest.Bools (SortableBoolTestDto.toDomain boolTestDto)
        | Uint8v256 uint8v256TestDto -> 
            sortableTest.Uint8v256 (SortableUint8v256TestDto.toDomain uint8v256TestDto)
        | Uint8v512 uint8v512TestDto ->
            sortableTest.Uint8v512 (SortableUint8v512TestDto.toDomain uint8v512TestDto)