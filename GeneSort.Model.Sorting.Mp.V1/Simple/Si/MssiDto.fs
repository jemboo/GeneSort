
namespace GeneSort.Model.Mp.Sorting.Mp.V1.Simple.Si

open System
open FSharp.UMX
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple.Si
open GeneSort.Core.Mp

type mssiDto =
    { id: Guid
      sortingWidth: int
      permSiDtos: permSiDto array }

module MssiDto =

    let fromDomain (mssi: mssi) : mssiDto =
        let mssiDtos = mssi.Perm_Sis |> Array.map PermSiDto.fromDomain
        { id = %mssi.Id
          sortingWidth = %mssi.SortingWidth
          permSiDtos = mssiDtos }

    let toDomain (mssiDto: mssiDto) : mssi =
        let perm_Sis = mssiDto.permSiDtos |> Array.map (PermSiDto.toDomain)
        mssi.create
            (UMX.tag<sorterModelId> mssiDto.id)
            (UMX.tag<sortingWidth> mssiDto.sortingWidth)
            perm_Sis
