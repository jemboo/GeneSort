module Program

open Xunit
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Mp.Sorter

[<Theory>]
[<InlineData(0, 0)>]
[<InlineData(0, 3)>]
[<InlineData(2, 7)>]
let ``Comparator DTO preserves both endpoints`` low high =
    let original = ce.create low high
    let restored = original |> CeDto.fromDomain |> CeDto.toDomain
    Assert.Equal(original, restored)
