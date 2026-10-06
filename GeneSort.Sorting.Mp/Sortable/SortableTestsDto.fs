namespace GeneSort.Sorting.Mp.Sortable

open MessagePack
open GeneSort.Sorting.Sortable

[<MessagePackObject>]
type sortableTestsDto =
    | Ints of sortableIntTestDto
    | Bools of sortableBoolTestDto
    | Uint8v256 of sortableUint8v256TestDto
    | Uint8v512 of sortableUint8v512TestDto
    | Bitv512 of sortableBitv512TestDto


module SortableTestsDto =

    let fromDomain (sorterTest: sortableTests) : sortableTestsDto =
        match sorterTest with
        | sortableTests.Ints intTest -> Ints (SortableIntTestDto.fromDomain intTest)
        | sortableTests.Bools boolTest -> Bools (SortableBoolTestDto.fromDomain boolTest)
        | sortableTests.Uint8v256 uint8v256Test -> 
            Uint8v256 (SortableUint8v256TestDto.fromDomain uint8v256Test)
        | sortableTests.Uint8v512 uint8v512Test -> 
            Uint8v512 (SortableUint8v512TestDto.fromDomain uint8v512Test)
        | sortableTests.Bitv512 bitv512Test  ->
            Bitv512 (SortableBitv512TestDto.fromDomain bitv512Test)
        | _ -> failwith "Unsupported sortableTests variant for DTO conversion."

    let toDomain (dto: sortableTestsDto) : sortableTests =
        match dto with
        | Ints intTestDto -> sortableTests.Ints (SortableIntTestDto.toDomain intTestDto)
        | Bools boolTestDto -> sortableTests.Bools (SortableBoolTestDto.toDomain boolTestDto)
        | Uint8v256 uint8v256TestDto -> 
            sortableTests.Uint8v256 (SortableUint8v256TestDto.toDomain uint8v256TestDto)
        | Uint8v512 uint8v512TestDto ->
            sortableTests.Uint8v512 (SortableUint8v512TestDto.toDomain uint8v512TestDto)
        | Bitv512 bitv512TestDto ->
            sortableTests.Bitv512 (SortableBitv512TestDto.toDomain bitv512TestDto)