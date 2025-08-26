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
    let toDto (sorterTest: sorterTest) : sorterTestDto =
        match sorterTest with
        | sorterTest.Ints intTest -> Ints (SorterIntTestDto.toDto intTest)
        | sorterTest.Bools boolTest -> Bools (SorterBoolTestDto.toDto boolTest)

    let fromDto (dto: sorterTestDto) : sorterTest =
        match dto with
        | Ints intTestDto -> sorterTest.Ints (SorterIntTestDto.fromDto intTestDto)
        | Bools boolTestDto -> sorterTest.Bools (SorterBoolTestDto.fromDto boolTestDto)