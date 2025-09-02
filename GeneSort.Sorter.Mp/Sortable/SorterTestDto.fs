namespace GeneSort.Sorter.Mp.Sortable

open System
open FSharp.UMX
open MessagePack
open GeneSort.Core
open GeneSort.Core.Mp
open GeneSort.Sorter
open GeneSort.Sorter.Sortable

[<MessagePackObject>]
type sorterTestDto =
    | Ints of sorterIntTestDto
    | Bools of sorterBoolTestDto

module SorterTestDto =

    let fromDomain (sorterTest: sorterTests) : sorterTestDto =
        match sorterTest with
        | sorterTests.Ints intTest -> Ints (SorterIntTestDto.fromDomain intTest)
        | sorterTests.Bools boolTest -> Bools (SorterBoolTestDto.fromDomain boolTest)

    let toDomain (dto: sorterTestDto) : sorterTests =
        match dto with
        | Ints intTestDto -> sorterTests.Ints (SorterIntTestDto.toDomain intTestDto)
        | Bools boolTestDto -> sorterTests.Bools (SorterBoolTestDto.toDomain boolTestDto)