namespace GeneSort.Sorting.Mp.Sortable

open MessagePack
open GeneSort.Sorting.Sortable

[<MessagePackObject>]
type sortableTestsDto =
    | Ints of sortableIntTestsDto
    | Bools of sortableBoolTestsDto
    | Uint8v256 of sortableUint8v256TestsDto
    | Uint8v512 of sortableUint8v512TestsDto
    | Bitv512 of sortableBitv512TestsDto


module SortableTestsDto =

    let fromDomain (sorterTest: sortableTests) : sortableTestsDto =
        match sorterTest with
        | sortableTests.Ints intTest -> Ints (SortableIntTestsDto.fromDomain intTest)
        | sortableTests.Bools boolTest -> Bools (SortableBoolTestsDto.fromDomain boolTest)
        | sortableTests.Uint8v256 uint8v256Test -> 
            Uint8v256 (SortableUint8v256TestsDto.fromDomain uint8v256Test)
        | sortableTests.Uint8v512 uint8v512Test -> 
            Uint8v512 (SortableUint8v512TestsDto.fromDomain uint8v512Test)
        | sortableTests.Bitv512 bitv512Test  ->
            Bitv512 (SortableBitv512TestsDto.fromDomain bitv512Test)
        | _ -> failwith "Unsupported sortableTests variant for DTO conversion."

    let toDomain (dto: sortableTestsDto) : sortableTests =
        match dto with
        | Ints intTestDto -> sortableTests.Ints (SortableIntTestsDto.toDomain intTestDto)
        | Bools boolTestDto -> sortableTests.Bools (SortableBoolTestsDto.toDomain boolTestDto)
        | Uint8v256 uint8v256TestDto -> 
            sortableTests.Uint8v256 (SortableUint8v256TestsDto.toDomain uint8v256TestDto)
        | Uint8v512 uint8v512TestDto ->
            sortableTests.Uint8v512 (SortableUint8v512TestsDto.toDomain uint8v512TestDto)
        | Bitv512 bitv512TestDto ->
            sortableTests.Bitv512 (SortableBitv512TestsDto.toDomain bitv512TestDto)