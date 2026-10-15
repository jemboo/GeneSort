namespace GeneSort.Sorting.Mp.Sortable

open MessagePack
open GeneSort.Sorting.Sortable
open GeneSort.Sorting.Sorter
open GeneSort.Sorting.Mp.Sorter

[<MessagePackObject>]
type sortableTestsWithPrefixDto =
    { [<Key(0)>] Tests: sortableTestsDto
      [<Key(1)>] Prefix: ceDto array }

module SortableTestsWithPrefixDto =
    let fromDomain (tests: sortableTests, prefix: ce array) : sortableTestsWithPrefixDto =
        { Tests = SortableTestsDto.fromDomain tests
          Prefix = prefix |> Array.map CeDto.fromDomain }

    let toDomain (dto: sortableTestsWithPrefixDto) : sortableTests * ce array =
        SortableTestsDto.toDomain dto.Tests, dto.Prefix |> Array.map CeDto.toDomain
