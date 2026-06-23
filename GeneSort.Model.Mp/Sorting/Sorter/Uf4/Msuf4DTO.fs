namespace GeneSort.Model.Mp.Sorting.Sorter.Uf4

open System
open FSharp.UMX
open MessagePack
open GeneSort.Sorting
open GeneSort.Core.Mp.TwoOrbitUnfolder
open GeneSort.Model.Sorting.Sorter.Uf4
open GeneSort.Model.Sorting

[<MessagePackObject>]
type msuf4Dto =
    { [<Key(0)>] id: Guid
      [<Key(1)>] sortingWidth: int
      [<Key(2)>] twoOrbitUf4Dtos: twoOrbitUf4Dto array }
    
    static member Create(id: Guid, sortingWidth: int, twoOrbitUnfolder4s: twoOrbitUf4Dto array) : msuf4Dto =
        if isNull twoOrbitUnfolder4s then
            raise (ArgumentNullException(nameof twoOrbitUnfolder4s, "TwoOrbitUnfolder4s array cannot be null"))
        elif twoOrbitUnfolder4s.Length < 1 then
            raise (ArgumentException($"Must have at least 1 TwoOrbitUnfolder4, got {twoOrbitUnfolder4s.Length}", nameof twoOrbitUnfolder4s))
        elif sortingWidth < 1 then
            raise (ArgumentException($"SortingWidth must be at least 1, got {sortingWidth}", nameof sortingWidth))
        elif twoOrbitUnfolder4s |> Array.exists (fun tou -> (tou |> TwoOrbitUf4Dto.getOrder) <> sortingWidth) then
            raise (ArgumentException($"All TwoOrbitUnfolder4 must have order {sortingWidth}", nameof twoOrbitUnfolder4s))
        else
            { id = id
              sortingWidth = sortingWidth
              twoOrbitUf4Dtos = twoOrbitUnfolder4s }

module Msuf4Dto =

    let fromDomain (msuf4: msuf4) : msuf4Dto =
        { id = %msuf4.Id
          sortingWidth = %msuf4.SortingWidth
          twoOrbitUf4Dtos = msuf4.TwoOrbitUnfolder4s |> Array.map TwoOrbitUf4Dto.fromDomain }

    let toDomain (dto: msuf4Dto) : msuf4 =
        let twoOrbitUnfolder4s = 
            dto.twoOrbitUf4Dtos 
            |> Array.map TwoOrbitUf4Dto.toDomain
        
        msuf4.create
            (UMX.tag<sorterModelId> dto.id)
            (UMX.tag<sortingWidth> dto.sortingWidth)
            twoOrbitUnfolder4s