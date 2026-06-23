namespace GeneSort.Model.Mp.Sorting.Sorter.Uf6

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorting
open GeneSort.Core.Mp.TwoOrbitUnfolder
open GeneSort.Model.Sorting.Sorter.Uf6
open GeneSort.Model.Sorting

[<MessagePackObject>]
type msuf6Dto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] sortingWidth: int
      [<Key(2)>] twoOrbitUf6Dtos: twoOrbitUf6Dto array }
    
    static member Create(id: Guid, sortingWidth: int, twoOrbitUnfolder6s: twoOrbitUf6Dto array) : msuf6Dto =
        if isNull twoOrbitUnfolder6s then
            raise (ArgumentNullException(nameof twoOrbitUnfolder6s, "TwoOrbitUnfolder6s array cannot be null"))
        elif twoOrbitUnfolder6s.Length < 1 then
            raise (ArgumentException($"Must have at least 1 TwoOrbitUnfolder6, got {twoOrbitUnfolder6s.Length}", nameof twoOrbitUnfolder6s))
        elif sortingWidth < 1 then
            raise (ArgumentException($"SortingWidth must be at least 1, got {sortingWidth}", nameof sortingWidth))
        elif twoOrbitUnfolder6s |> Array.exists (fun tou -> (tou |> TwoOrbitUf6Dto.getOrder) <> sortingWidth) then
            raise (ArgumentException($"All TwoOrbitUnfolder6 must have order {sortingWidth}", nameof twoOrbitUnfolder6s))
        else
            { id = id
              sortingWidth = sortingWidth
              twoOrbitUf6Dtos = twoOrbitUnfolder6s }

module Msuf6Dto =

    let fromDomain (msuf6: msuf6) : msuf6Dto =
        { id = %msuf6.Id
          sortingWidth = %msuf6.SortingWidth
          twoOrbitUf6Dtos = msuf6.TwoOrbitUnfolder6s |> Array.map TwoOrbitUf6Dto.fromDomain }

    let toDomain (dto: msuf6Dto) : msuf6 =
        let twoOrbitUnfolder6s = 
            dto.twoOrbitUf6Dtos 
            |> Array.map TwoOrbitUf6Dto.toDomain
        
        msuf6.create
            (UMX.tag<sorterModelId> dto.id)
            (UMX.tag<sortingWidth> dto.sortingWidth)
            twoOrbitUnfolder6s