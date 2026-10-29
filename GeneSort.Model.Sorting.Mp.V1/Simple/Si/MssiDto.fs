
namespace GeneSort.Model.Sorting.Mp.V1.Simple.Si

open System
open FSharp.UMX
open GeneSort.Core.Mp
open GeneSort.Sorting
open GeneSort.Model.Sorting.V1
open GeneSort.Model.Sorting.V1.Simple.Si

type mssiDto =
    { id: Guid
      channelCount: int
      permSiDtos: permSiDto array }

module MssiDto =

    let fromDomain (mssi: mssi) : mssiDto =
        let mssiDtos = mssi.Perm_Sis |> Array.map PermSiDto.fromDomain
        { id = %mssi.Id
          channelCount = %mssi.ChannelCount
          permSiDtos = mssiDtos }

    let toDomain (mssiDto: mssiDto) : mssi =
        let perm_Sis = mssiDto.permSiDtos |> Array.map (PermSiDto.toDomain)
        mssi.create
            (UMX.tag<sorterModelId> mssiDto.id)
            (UMX.tag<channelCount> mssiDto.channelCount)
            perm_Sis
